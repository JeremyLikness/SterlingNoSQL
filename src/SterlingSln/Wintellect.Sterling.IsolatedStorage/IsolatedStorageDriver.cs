using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Wintellect.Sterling.Database;
using Wintellect.Sterling.Serialization;

namespace Wintellect.Sterling.IsolatedStorage
{
    /// <summary>
    ///     Default driver for isolated storage
    /// </summary>
    public class IsolatedStorageDriver : BaseDriver
    {
        private const string BASE = "Sterling/";
        private readonly List<Type> _tables = new List<Type>();
        private bool _dirtyType;
        private readonly Dictionary<string,byte[]> _saveCache = new Dictionary<string, byte[]>();
        
        public IsolatedStorageDriver() : this(BASE, false)
        {            
        }

        public IsolatedStorageDriver(string basePath) : this(basePath, false)
        {            
        }

        public IsolatedStorageDriver(string basePath, bool siteWide)
        {       
            Initialize(basePath, siteWide);
        }

        public IsolatedStorageDriver(string databaseName, ISterlingSerializer serializer, Action<SterlingLogLevel, string, Exception> log) : this(databaseName, serializer, log, false)
        {
        }

        public IsolatedStorageDriver(string databaseName, ISterlingSerializer serializer, Action<SterlingLogLevel, string, Exception> log, bool siteWide) 
            : this(databaseName, serializer, log, siteWide, BASE)
        {           
        }

        public IsolatedStorageDriver(string databaseName, ISterlingSerializer serializer, Action<SterlingLogLevel, string, Exception> log, bool siteWide, string basePath)
            : base(databaseName, serializer, log)
        {
            Initialize(basePath, siteWide);
        }

        private IsoStorageHelper _iso;
        private string _basePath;
        private readonly PathProvider _pathProvider = new PathProvider();

        public void Initialize(string basePath, bool siteWide)
        {
            _iso = new IsoStorageHelper(siteWide);            
            _basePath = basePath; 
        }

        /// <summary>
        ///     Serialize the keys
        /// </summary>
        /// <param name="type">Type of the parent table</param>
        /// <param name="keyType">Type of the key</param>
        /// <param name="keyMap">Key map</param>
        public override void SerializeKeys(Type type, Type keyType, IDictionary keyMap)
        {
            _iso.EnsureDirectory(_pathProvider.GetTablePath(_basePath, DatabaseName, type, this));
            var pathLock = PathLock.GetLock(type.FullName);
            lock(pathLock)
            {
                var keyPath = _pathProvider.GetKeysPath(_basePath, DatabaseName, type, this);
                using (var keyFile = _iso.GetWriter(keyPath))
                {
                    keyFile.Write(keyMap.Count);
                    foreach(var key in keyMap.Keys)
                    {
                        DatabaseSerializer.Serialize(key, keyFile);
                        keyFile.Write((int)keyMap[key]);
                    }
                }
            }
            SerializeTypes();
        }

        /// <summary>
        ///     Deserialize the keys
        /// </summary>
        /// <param name="type">Type of the parent table</param>
        /// <param name="keyType">Type of the key</param>
        /// <param name="dictionary">Empty dictionary</param>
        /// <returns>The key list</returns>
        public override IDictionary DeserializeKeys(Type type, Type keyType, IDictionary dictionary)
        {
            var keyPath = _pathProvider.GetKeysPath(_basePath, DatabaseName, type, this);
            if (_iso.FileExists(keyPath))
            {
                var pathLock = PathLock.GetLock(type.FullName);
                lock (pathLock)
                {
                    using (var keyFile = _iso.GetReader(keyPath))
                    {
                        var count = keyFile.ReadInt32();
                        for (var x = 0; x < count; x++)
                        {
                            dictionary.Add(DatabaseSerializer.Deserialize(keyType, keyFile),
                                           keyFile.ReadInt32());
                        }
                    }
                }
            }
            return dictionary;
        }

        /// <summary>
        ///     Serialize a single index 
        /// </summary>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <typeparam name="TIndex">The type of the index</typeparam>
        /// <param name="type">The type of the parent table</param>
        /// <param name="indexName">The name of the index</param>
        /// <param name="indexMap">The index map</param>
        public override void SerializeIndex<TKey, TIndex>(Type type, string indexName, Dictionary<TKey, TIndex> indexMap)
        {
            var indexPath = _pathProvider.GetIndexPath(_basePath, DatabaseName, type, this, indexName);
            var pathLock = PathLock.GetLock(type.FullName);
            lock(pathLock)
            {
                using (var indexFile = _iso.GetWriter(indexPath))
                {
                    indexFile.Write(indexMap.Count);
                    foreach(var index in indexMap)
                    {
                        DatabaseSerializer.Serialize(index.Value, indexFile);
                        DatabaseSerializer.Serialize(index.Key, indexFile);                        
                    }
                }
            }
        }

        /// <summary>
        ///     Serialize a double index 
        /// </summary>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <typeparam name="TIndex1">The type of the first index</typeparam>
        /// <typeparam name="TIndex2">The type of the second index</typeparam>
        /// <param name="type">The type of the parent table</param>
        /// <param name="indexName">The name of the index</param>
        /// <param name="indexMap">The index map</param>        
        public override void SerializeIndex<TKey, TIndex1, TIndex2>(Type type, string indexName, Dictionary<TKey, Tuple<TIndex1, TIndex2>> indexMap)
        {
            var indexPath = _pathProvider.GetIndexPath(_basePath, DatabaseName, type, this, indexName);
            var pathLock = PathLock.GetLock(type.FullName);
            lock (pathLock)
            {
                using (var indexFile = _iso.GetWriter(indexPath))
                {
                    indexFile.Write(indexMap.Count);
                    foreach (var index in indexMap)
                    {
                        DatabaseSerializer.Serialize(index.Value.Item1, indexFile);
                        DatabaseSerializer.Serialize(index.Value.Item2, indexFile);
                        DatabaseSerializer.Serialize(index.Key, indexFile);                        
                    }
                }
            }
        }

        /// <summary>
        ///     Deserialize a single index
        /// </summary>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <typeparam name="TIndex">The type of the index</typeparam>
        /// <param name="type">The type of the parent table</param>
        /// <param name="indexName">The name of the index</param>        
        /// <returns>The index map</returns>
        public override Dictionary<TKey, TIndex> DeserializeIndex<TKey, TIndex>(Type type, string indexName)
        {
            var indexPath = _pathProvider.GetIndexPath(_basePath, DatabaseName, type, this, indexName);
            var dictionary = new Dictionary<TKey, TIndex>();
            if (_iso.FileExists(indexPath))
            {
                var pathLock = PathLock.GetLock(type.FullName);
                lock (pathLock)
                {
                    using (var indexFile = _iso.GetReader(indexPath))
                    {
                        var count = indexFile.ReadInt32();
                        for (var x = 0; x < count; x++)
                        {
                            var index = (TIndex) DatabaseSerializer.Deserialize(typeof (TIndex), indexFile);
                            var key = (TKey) DatabaseSerializer.Deserialize(typeof (TKey), indexFile);
                            dictionary.Add(key, index);
                        }
                    }
                }
            }
            return dictionary;
        }

        /// <summary>
        ///     Deserialize a double index
        /// </summary>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <typeparam name="TIndex1">The type of the first index</typeparam>
        /// <typeparam name="TIndex2">The type of the second index</typeparam>
        /// <param name="type">The type of the parent table</param>
        /// <param name="indexName">The name of the index</param>        
        /// <returns>The index map</returns>        
        public override Dictionary<TKey, Tuple<TIndex1, TIndex2>> DeserializeIndex<TKey, TIndex1, TIndex2>(Type type, string indexName)
        {
            var indexPath = _pathProvider.GetIndexPath(_basePath, DatabaseName, type, this, indexName);
            var dictionary = new Dictionary<TKey, Tuple<TIndex1, TIndex2>>();
            if (_iso.FileExists(indexPath))
            {
                var pathLock = PathLock.GetLock(type.FullName);
                lock (pathLock)
                {
                    using (var indexFile = _iso.GetReader(indexPath))
                    {
                        var count = indexFile.ReadInt32();
                        for (var x = 0; x < count; x++)
                        {
                            var index = Tuple.Create(
                                (TIndex1) DatabaseSerializer.Deserialize(typeof (TIndex1), indexFile),
                                (TIndex2) DatabaseSerializer.Deserialize(typeof (TIndex2), indexFile));
                            var key = (TKey) DatabaseSerializer.Deserialize(typeof (TKey), indexFile);
                            dictionary.Add(key, index);
                        }
                    }
                }
            }
            return dictionary;
        }

        /// <summary>
        ///     Publish the list of tables
        /// </summary>
        /// <param name="tables">The list of tables</param>
        public override void PublishTables(Dictionary<Type, ITableDefinition> tables)
        {
            _iso.EnsureDirectory(_pathProvider.GetDatabasePath(_basePath, DatabaseName, this));

            var typePath = _pathProvider.GetTypesPath(_basePath, DatabaseName, this);

            if (!_iso.FileExists(typePath)) return;

            using (var typeFile = _iso.GetReader(typePath))
            {
                var count = typeFile.ReadInt32();
                for (var x = 0; x < count; x++)
                {
                    GetTypeIndex(typeFile.ReadString());
                }
            }

            var pathLock = PathLock.GetLock(DatabaseName);
            lock (pathLock)
            {
                foreach (var type in tables.Keys)
                {
                    _tables.Add(type);
                    _iso.EnsureDirectory(_pathProvider.GetTablePath(_basePath, DatabaseName, type, this));
                }
            }
        }

        /// <summary>
        ///     Serialize the type master
        /// </summary>
        public override void SerializeTypes()
        {
            var pathLock = PathLock.GetLock(TypeIndex.GetType().FullName);            
            lock (pathLock)
            {
                var typePath = _pathProvider.GetTypesPath(_basePath, DatabaseName, this);
                using (var typeFile = _iso.GetWriter(typePath))
                {
                    typeFile.Write(TypeIndex.Count);
                    foreach (var type in TypeIndex)
                    {
                        typeFile.Write(type);
                    }
                }
            }
        }

        /// <summary>
        ///     Get the index for the type
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>The type</returns>
        public override int GetTypeIndex(string type)
        {
            var pathLock = PathLock.GetLock(TypeIndex.GetType().FullName);
            lock(pathLock)
            {
                if (!TypeIndex.Contains(type))
                {
                    TypeIndex.Add(type);
                    _dirtyType = true;
                }
            }
            return TypeIndex.IndexOf(type);
        }

        /// <summary>
        ///     Get the type at an index
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The type</returns>
        public override string GetTypeAtIndex(int index)
        {
            return TypeIndex[index];
        }
        
        /// <summary>
        ///     Save operation
        /// </summary>
        /// <param name="type">Type of the parent</param>
        /// <param name="keyIndex">Index for the key</param>
        /// <param name="bytes">The byte stream</param>
        public override void Save(Type type, int keyIndex, byte[] bytes)
        {            
            var instanceFolder = _pathProvider.GetInstanceFolder(_basePath, DatabaseName, type, this, keyIndex);
            _iso.EnsureDirectory(instanceFolder);
            var instancePath = _pathProvider.GetInstancePath(_basePath, DatabaseName, type, this, keyIndex);

            lock(((ICollection)_saveCache).SyncRoot)
            {
                _saveCache[instancePath] = bytes;
            }

            // lock on this while saving, but remember that anyone else loading can now grab the
            // copy 
            lock (PathLock.GetLock(instancePath))
            {
                using (
                    var instanceFile =
                        _iso.GetWriter(instancePath))
                {
                    instanceFile.Write(bytes);
                }

                lock (((ICollection)_saveCache).SyncRoot)
                {
                    if (_saveCache.ContainsKey(instancePath))
                    {
                        _saveCache.Remove(instancePath);
                    }
                }
            }

            if (!_dirtyType) return;
            
            _dirtyType = false;
            SerializeTypes();            
        }   
            
        /// <summary>
        ///     Load from the store
        /// </summary>
        /// <param name="type">The type of the parent</param>
        /// <param name="keyIndex">The index of the key</param>
        /// <returns>The byte stream</returns>
        public override BinaryReader Load(Type type, int keyIndex)
        {
            var instancePath = _pathProvider.GetInstancePath(_basePath, DatabaseName, type, this, keyIndex);

            // if a copy is available, return that - it's our best bet
            lock(((ICollection)_saveCache).SyncRoot)
            {
                if (_saveCache.ContainsKey(instancePath))
                {
                    return new BinaryReader(new MemoryStream(_saveCache[instancePath]));
                }
            }

            // otherwise let's wait for it to be released and grab it from disk
            lock (PathLock.GetLock(instancePath))
            {
                return _iso.FileExists(instancePath)
                           ? _iso.GetReader(instancePath)
                           : new BinaryReader(new MemoryStream());
            }
        }

        /// <summary>
        ///     Delete from the store
        /// </summary>
        /// <param name="type">The type of the parent</param>
        /// <param name="keyIndex">The index of the key</param>
        public override void Delete(Type type, int keyIndex)
        {
            var instancePath = _pathProvider.GetInstancePath(_basePath, DatabaseName, type, this, keyIndex);
            lock (PathLock.GetLock(instancePath))
            {
                if (_iso.FileExists(instancePath))
                {
                    _iso.Delete(instancePath);
                }
            }            
        }

        /// <summary>
        ///     Truncate a type
        /// </summary>
        /// <param name="type">The type to truncate</param>
        public override void Truncate(Type type)
        {
            var folderPath = _pathProvider.GetTablePath(_basePath, DatabaseName, type, this);
            lock(PathLock.GetLock(type.FullName))
            {
                _iso.Purge(folderPath);
            }
        }

        /// <summary>
        ///     Purge the database
        /// </summary>
        public override void Purge()
        {
            lock(PathLock.GetLock(DatabaseName))
            {
                _iso.Purge(_pathProvider.GetDatabasePath(_basePath, DatabaseName, this));
            }
        }        
    }
}