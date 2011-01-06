﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using Wintellect.Sterling.Exceptions;
using System.Threading;
using Wintellect.Sterling.Database;

namespace Wintellect.Sterling.IsolatedStorage
{
    /// <summary>
    ///     This class is used to assist with manager the isolated storage references, allowing
    ///     for nested requests to use the same isolated storage reference
    /// </summary>
    internal class IsoStorageHelper : IDisposable 
    {
        private static readonly List<string> _paths = new List<string>();
        private static readonly List<string> _files = new List<string>();

        /// <summary>
        ///     The isolated storage file reference
        /// </summary>
        private static IsolatedStorageFile _iso;
        
        /// <summary>
        ///     Constructor - determine whether or not to spin up the iso instance
        /// </summary>
        public IsoStorageHelper()
        {
            if (_iso != null) return;

            _iso = IsolatedStorageFile.GetUserStoreForApplication();
            Application.Current.Exit += (o, e) => _iso.Dispose();
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
                    if (_files.Contains(path))
                    {
                        _paths.Remove(path);
                    }
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
            if (path.EndsWith("/"))
            {
                path = path.Substring(0, path.Length - 1);
            }

            try
            {
                if (!_paths.Contains(path))
                {
                    if (!_iso.DirectoryExists(path))
                    {
                        _iso.CreateDirectory(path);
                    }
                    _paths.Add(path);
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
                if (_files.Contains(path))
                    return true; 

                if (_iso.FileExists(path))
                {
                    _files.Add(path);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new SterlingIsolatedStorageException(ex);
            }
        }

        /// <summary>
        /// Purge a directory and everything beneath it
        /// </summary>
        /// <param name="path">The path</param>
        public void Purge(string path)
        {
            Purge(path, true);
        }

        /// <summary>
        /// Purge a directory and everything beneath it
        /// </summary>
        /// <param name="path">The path</param>
        /// <param name="clear">A value indicating whether the internal lists should be cleared</param>
        private static void Purge(string path, bool clear)
        {
            if (clear)
            {
                _paths.Clear();
                _files.Clear();
            }

            try
            {
                // already purged!
                if (!_iso.DirectoryExists(path))
                {
                    return;
                }

                // clear the sub directories
                foreach (var dir in _iso.GetDirectoryNames(Path.Combine(path, "*")))
                {
                    Purge(Path.Combine(path, dir), false);
                }

                // clear the files - don't use a where clause because we want to get closer to the delete operation
                // with the filter
                foreach (var filePath in
                    _iso.GetFileNames(Path.Combine(path, "*"))
                    .Select(file => Path.Combine(path, file)))
                {
                    var pathLock = PathLock.GetLock(filePath.Replace("\\", "/"));
                    lock (pathLock)
                    {
                        _iso.DeleteFile(filePath);
                    }
                }

                var dirPath = path.TrimEnd('\\', '/');
                if (_iso.DirectoryExists(dirPath))
                {
                    _iso.DeleteDirectory(dirPath);                   
                }
            }
            catch (Exception ex)
            {
                throw new SterlingIsolatedStorageException(ex);
            }
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {            
        }             
    }
}
