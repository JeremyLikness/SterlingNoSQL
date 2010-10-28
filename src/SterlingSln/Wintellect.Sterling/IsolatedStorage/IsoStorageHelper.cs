using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using Wintellect.Sterling.Exceptions;

namespace Wintellect.Sterling.IsolatedStorage
{
    /// <summary>
    ///     This class is used to assist with manager the isolated storage references, allowing
    ///     for nested requests to use the same isolated storage reference
    /// </summary>
    internal class IsoStorageHelper : IDisposable, ISterlingLock  
    {
        /// <summary>
        ///     The isolated storage file reference
        /// </summary>
        private static IsolatedStorageFile _iso;

        /// <summary>
        ///     Reference count of requestors 
        /// </summary>
        private static int _refCount; 

        /// <summary>
        ///     Constructor - determine whether or not to spin up the iso instance
        /// </summary>
        public IsoStorageHelper()
        {
            lock(Lock)
            {
                if (_refCount == 0)
                {
                    _iso = IsolatedStorageFile.GetUserStoreForApplication();
                }
                _refCount++;
            }
        }

        /// <summary>
        ///     Gets an isolated storage reader
        /// </summary>
        /// <param name="path">The path</param>
        /// <returns>The reader</returns>
        public BinaryReader GetReader(string path)
        {
            try
            {
                return new BinaryReader(_iso.OpenFile(path, FileMode.Open, FileAccess.Read));
            }
            catch(Exception ex)
            {
                throw new SterlingIsolatedStorageException(ex);
            }
        }

        /// <summary>
        ///     Get an isolated storage writer
        /// </summary>
        /// <param name="path">The path</param>
        /// <returns>The writer</returns>
        public BinaryWriter GetWriter(string path)
        {
            try
            {
                return new BinaryWriter(_iso.OpenFile(path, FileMode.Create, FileAccess.Write));
            }
            catch(Exception ex)
            {
                throw new SterlingIsolatedStorageException(ex);
            }
        }

        /// <summary>
        ///     Delete a file based on its path
        /// </summary>
        /// <param name="path">The path</param>
        public void Delete(string path)
        {
            try
            {
                if (_iso.FileExists(path))
                {
                    _iso.DeleteFile(path);
                }
            }
            catch(Exception ex)
            {
                throw new SterlingIsolatedStorageException(ex);   
            }
        }
       
        /// <summary>
        ///     Ensure that a directory exists
        /// </summary>
        /// <param name="path">the path</param>
        public void EnsureDirectory(string path)
        {
            if (!path.EndsWith("/"))
            {
                path = path.Substring(0, path.Length - 1);
            }

            try
            {
                if (!_iso.DirectoryExists(path))
                {
                    _iso.CreateDirectory(path);
                }
            }
            catch(Exception ex)
            {
                throw new SterlingIsolatedStorageException(ex);
            }
        }

        /// <summary>
        ///     Check to see if a file exists
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <returns>True if it exists</returns>
        public bool FileExists(string path)
        {
            try
            {
                return _iso.FileExists(path);
            }
            catch (Exception ex)
            {
                throw new SterlingIsolatedStorageException(ex);
            }
        }

        /// <summary>
        ///     Purge a directory and everything beneath it
        /// </summary>
        /// <param name="path">The path</param>
        public void Purge(string path)
        {
            if (!path.EndsWith("/"))
            {
                path = path + "/";
            }

            try
            {
                // already purged!
                if (!_iso.DirectoryExists(path))
                {
                    return;
                }

                // sort by levels deep
                var list = (from d in _GetAllDirectories(path) orderby d.Count(c => c.Equals('/')) descending select d);

                foreach (var dir in list)
                {
                    foreach(var file in _GetAllFiles(dir))
                    {
                        _iso.DeleteFile(file);
                    }
                    _iso.DeleteDirectory(dir);
                }
                
                foreach (var file in _GetAllFiles(path).Where(file => _iso.FileExists(file)))
                {
                    _iso.DeleteFile(file);
                }

                _iso.DeleteDirectory(path);
            }
            catch(Exception ex)
            {
                throw new SterlingIsolatedStorageException(ex);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            lock(Lock)
            {
                if (--_refCount != 0) return;

                _iso.Dispose();
                _iso = null;
            }
        }

        // Method to retrieve all directories, recursively, within a store.
        private static string[] _GetAllDirectories(string pattern)
        {            
            // Retrieve directories.
            var directories = _iso.GetDirectoryNames(pattern + "*");

            var directoryList = directories.Select(dir => string.Format("{0}{1}/", pattern, dir)).ToList();

            // Retrieve subdirectories of matches.
            for (int i = 0, max = directories.Length; i < max; i++)
            {
                var directory = directoryList[i];
                var more = _GetAllDirectories(directory);                
                // Insert the subdirectories into the list and
                // update the counter and upper bound.
                directoryList.InsertRange(i + 1, more);
                i += more.Length;
                max += more.Length;
            }

            return directoryList.ToArray();
        }

        /// <summary>
        ///     All files
        /// </summary>
        /// <param name="dir">Base directory</param>
        /// <returns></returns>
        private static IEnumerable<string> _GetAllFiles(string dir)
        {
            if (!dir.EndsWith("/"))
            {
                dir += "/";
            }

            var pattern = dir + "*";

            return _iso.GetFileNames(pattern).Select(file => string.Format("{0}{1}", dir, file)).ToList(); 
        } 

        private static readonly object _lock = new object();

        public object Lock
        {
            get { return _lock; }
        }
    }
}
