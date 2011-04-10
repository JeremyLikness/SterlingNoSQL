using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using Path = System.IO.Path;

namespace Wintellect.Sterling.IsolatedStorageUpgrade
{
    public static class Upgrade
    {
        private static BackgroundWorker _upgradeWorker;
        private const string STERLING_ROOT = @"Sterling\";
        private const string DATABASES = "db.dat";
        private const string TYPES = "types.dat";

        private static IsolatedStorageFile _iso;

        public static bool CancelUpgrade()
        {
            if (_upgradeWorker != null)
            {
                _upgradeWorker.CancelAsync();
                return true;
            }
            return false;
        }

        public static void DoUpgrade(Action upgradeCompleted)
        {
            if (_upgradeWorker != null)
            {
                throw new Exception("Upgrade already in progress.");
            }

            _iso = IsolatedStorageFile.GetUserStoreForApplication();

            var path = string.Format("{0}{1}", STERLING_ROOT, DATABASES);
            if (!_iso.FileExists(path))
            {
                upgradeCompleted();
                return;
            }

            _upgradeWorker = new BackgroundWorker {WorkerSupportsCancellation = true};
            _upgradeWorker.DoWork += _UpgradeWorkerDoWork;
            _upgradeWorker.RunWorkerCompleted += (o, e) =>
                                                     {
                                                         _upgradeWorker = null;
                                                         upgradeCompleted();
                                                     };
            _upgradeWorker.RunWorkerAsync();
        }

        private static void _UpgradeWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            _iso = IsolatedStorageFile.GetUserStoreForApplication();
            e.Cancel = !_IterateDatabases();            
            _iso.Dispose();
            _iso = null;
        }

        private static bool _IterateDatabases()
        {
            var path = string.Format("{0}{1}", STERLING_ROOT, DATABASES);
            if (!_iso.FileExists(path))
            {
                return true;
            }

            using (var br = new BinaryReader(_iso.OpenFile(path, FileMode.Open, FileAccess.Read)))
            {                
                br.ReadInt32(); // next database
                br.ReadInt32(); // next table 
                var count = br.ReadInt32();
                for (var i = 0; i < count; i++)
                {
                    if (_upgradeWorker.CancellationPending)
                    {
                        return false;
                    }

                    var dbName = br.ReadString();
                    var dbIndex = br.ReadInt32();
                    if (!_ProcessDatabase(dbIndex, dbName))
                    {
                        return false;
                    }
                }                
            }

            _purgeQueue.Enqueue(PurgeEntry.CreateEntry(false, path));
            _purgeQueue.Enqueue(PurgeEntry.CreateEntry(false, string.Format("@{0}{1}", STERLING_ROOT, TYPES)));

            if (!_Purge())
            {
                return false;
            }

            return true;
        }

        private static bool _ProcessDatabase(int dbIndex, string dbName)
        {
            var dbPath = string.Format("{0}{1}", STERLING_ROOT, dbIndex);
            var newDbPath = string.Format("{0}{1}", STERLING_ROOT, dbName.GetHashCode());

            if (!_iso.DirectoryExists(dbPath))
            {
                return true;
            }

            if (!_iso.DirectoryExists(newDbPath))
            {
                _iso.CreateDirectory(newDbPath);
            }

            var typeSource = string.Format("{0}{1}", STERLING_ROOT, TYPES);
            var typeTarget = string.Format(@"{0}\{1}", newDbPath, TYPES);

            _iso.CopyFile(typeSource, typeTarget, true);

            _purgeQueue.Clear();

            if (!_Copy(dbPath, newDbPath, dbPath))
            {
                return false; 
            }

            return _Purge();
        }
        
        private static readonly Queue<PurgeEntry> _purgeQueue = new Queue<PurgeEntry>();

        private static bool _Copy(string root, string targetRoot, string path)
        {
            // already copied
            if (!_iso.DirectoryExists(path))
            {
                return true;
            }

            var targetDirectory = path.Replace(root, targetRoot);

            if (!_iso.DirectoryExists(targetDirectory))
            {
                _iso.CreateDirectory(targetDirectory);
            }            

            // clear the sub directories)
            foreach (var dir in _iso.GetDirectoryNames(Path.Combine(path, "*")))
            {
                if (_upgradeWorker.CancellationPending)
                {
                    return false;
                }
                if (!_Copy(root, targetRoot, Path.Combine(path, dir)))
                {
                    return false;
                }
            }

            // clear the files - don't use a where clause because we want to get closer to the delete operation
            // with the filter
            foreach (var filePath in
                _iso.GetFileNames(Path.Combine(path, "*"))
                    .Select(file => Path.Combine(path, file)))
            {
                if (_upgradeWorker.CancellationPending)
                {
                    return false;
                }                

                // ignore indexes
                var target = filePath.Replace(root, targetRoot);
                if (_iso.FileExists(filePath))
                {
                    _iso.CopyFile(filePath, target, true);
                }
                _purgeQueue.Enqueue(PurgeEntry.CreateEntry(false, filePath));
            }

            var dirPath = path.TrimEnd('\\', '/');
            _purgeQueue.Enqueue(PurgeEntry.CreateEntry(true, dirPath));

            return true;
        }

        private static bool _Purge()
        {
            while(_purgeQueue.Count > 0)
            {
                if (_upgradeWorker.CancellationPending)
                {
                    return false;
                }
                var entry = _purgeQueue.Dequeue();
                if (entry.IsDirectory && _iso.DirectoryExists(entry.Path))
                {
                    _iso.DeleteDirectory(entry.Path);
                }
                else if (_iso.FileExists(entry.Path))
                {
                    _iso.DeleteFile(entry.Path);
                }
            }
            return true;
        }

    }
}