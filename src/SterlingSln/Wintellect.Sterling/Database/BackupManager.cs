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

        /// <summary>
        ///     Backup 
        /// </summary>
        /// <param name="stream">A stream to backup to</param>
        /// <param name="path">Path to recruse</param>
        /// <param name="first">True for first pass</param>
        private void Backup(BinaryWriter stream, string path, bool first)
        {
            try
            {
                stream.Write(DIRECTORY);

                // write the original path (remember, on restore, it may be a completely different path)
                // just the first time, then it's partial paths
                stream.Write(first ? path : path.Replace(_rootPath, string.Empty));

                // already purged!
                if (!_iso.DirectoryExists(path))
                {
                    return;
                }

                // recurse subdirectories
                foreach (var dir in _iso.GetDirectoryNames(Path.Combine(path, "*")))
                {
                    Backup(stream, Path.Combine(path, dir), false);
                }

                // iterate files
                foreach (var filePath in
                    _iso.GetFileNames(Path.Combine(path, "*"))
                    .Select(file => Path.Combine(path, file)))
                {
                    // write the file and original path
                    stream.Write(FILE);
                    stream.Write(filePath);

                    // now parse the file, write the length and then the bitstream
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
        
        /// <summary>
        ///     Restore
        /// </summary>
        /// <param name="reader">A reader to read from</param>
        /// <param name="path">The path to restore to</param>
        public void Restore(BinaryReader reader, string path)
        {
            _rootPath = path;
            var oldPath = string.Empty; 

            // first command, should be a directory 
            var command = reader.ReadByte();
            var first = true;

            // until end
            while(command != DONE)
            {
                // process directories - just make sure they exist
                if (command.Equals(DIRECTORY))
                {
                    var directory = reader.ReadString();

                    // first time get the old path so we can swap it for the files
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
                    // get the file then replace it with the new path
                    var srcFile = reader.ReadString();
                    var filePath = srcFile.Replace(oldPath, _rootPath);

                    // make a buffer to accept the file
                    var buffer = new byte[reader.ReadInt32()];

                    // read it
                    buffer = reader.ReadBytes(buffer.Length);

                    // write it
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