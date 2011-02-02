using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using Wintellect.Sterling.Exceptions;

namespace Wintellect.Sterling.Database
{
    public class BackupManager
    {
        private readonly IsolatedStorageFile _iso = IsolatedStorageFile.GetUserStoreForApplication();
        
        private string _rootPath = string.Empty;
        private const byte DIRECTORY = 0x0;
        private const byte FILE = 0x1;
        private const byte DONE = 0x2;

        public void Backup(BinaryWriter writer, string path)
        {
            _rootPath = path; 
            Backup(writer, path, true);
            writer.Write(DONE);
        }

        private void Backup(BinaryWriter stream, string path, bool first)
        {
            try
            {
                stream.Write(DIRECTORY);
                stream.Write(first ? path : path.Replace(_rootPath, string.Empty));

                // already purged!
                if (!_iso.DirectoryExists(path))
                {
                    return;
                }

                // clear the sub directories
                foreach (var dir in _iso.GetDirectoryNames(Path.Combine(path, "*")))
                {
                    Backup(stream, Path.Combine(path, dir), false);
                }

                // clear the files - don't use a where clause because we want to get closer to the delete operation
                // with the filter
                foreach (var filePath in
                    _iso.GetFileNames(Path.Combine(path, "*"))
                    .Select(file => Path.Combine(path, file)))
                {
                    stream.Write(FILE);
                    stream.Write(filePath);
                    byte[] fileBytes;
                    using (var fileStream = _iso.OpenFile(filePath, FileMode.Open, FileAccess.Read))
                    {
                        fileBytes = new byte[fileStream.Length];
                        fileStream.Read(fileBytes, 0, fileBytes.Length);
                    }
                    stream.Write(fileBytes.Length);
                    stream.Write(fileBytes);                    
                }                
            }
            catch (Exception ex)
            {
                throw new SterlingIsolatedStorageException(ex);
            }
        }    
        
        public void Restore(BinaryReader reader, string path)
        {
            _rootPath = path;
            var oldPath = string.Empty; 
            var command = reader.ReadByte();
            var first = true;
            while(command != DONE)
            {
                if (command.Equals(DIRECTORY))
                {
                    var directory = reader.ReadString();
                    if (first)
                    {
                        oldPath = directory;
                        first = false;
                    }
                    else
                    {
                        var targetDirectory = Path.Combine(path, directory);
                        if (!_iso.DirectoryExists(targetDirectory))
                        {
                            _iso.CreateDirectory(targetDirectory);
                        }
                    }
                }
                else
                {
                    var srcFile = reader.ReadString();
                    var filePath = srcFile.Replace(oldPath, _rootPath);
                    var buffer = new byte[reader.ReadInt32()];
                    buffer = reader.ReadBytes(buffer.Length);
                    using (var fileStream = _iso.OpenFile(filePath, FileMode.Create, FileAccess.Write))
                    {
                        fileStream.Write(buffer, 0, buffer.Length);
                    }
                }
                command = reader.ReadByte();
            }
        }
    }    
}