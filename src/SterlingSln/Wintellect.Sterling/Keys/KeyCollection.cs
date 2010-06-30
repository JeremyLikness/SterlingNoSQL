using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wintellect.Sterling.Exceptions;
using Wintellect.Sterling.IsolatedStorage;
using Wintellect.Sterling.Serialization;

namespace Wintellect.Sterling.Keys
{
    /// <summary>
    ///     Collection of keys for a given entity
    /// </summary>
    internal class KeyCollection<T,TKey> : IKeyCollection where T: class, new()
    {
        private readonly PathProvider _pathProvider;
        private readonly string _databaseName;
        private readonly Func<TKey, T> _resolver;
        private readonly ISterlingSerializer _serializer;
        
        /// <summary>
        ///     Set when keys change
        /// </summary>
        public bool IsDirty { get; private set; }

        /// <summary>
        ///     Initialize the key collection
        /// </summary>
        /// <param name="pathProvider">Path provider</param>
        /// <param name="databaseName">Name of the database</param>
        /// <param name="serializer">The serializer it can use to write/restore keys</param>
        /// <param name="resolver">The resolver for loading the object</param>
        public KeyCollection(PathProvider pathProvider, string databaseName, ISterlingSerializer serializer, Func<TKey,T> resolver)
        {
            _pathProvider = pathProvider;
            _resolver = resolver;
            _databaseName = databaseName;
            _serializer = serializer;

            if (!serializer.CanSerialize<TKey>())
            {
                throw new SterlingSerializerException(serializer,typeof(TKey));
            }

            _DeserializeKeys();

            IsDirty = false;        
        }        

        /// <summary>
        ///     The list of keys
        /// </summary>
        private readonly List<TableKey<T,TKey>> _keyList = new List<TableKey<T, TKey>>();

        /// <summary>
        ///     Map for keys in the set
        /// </summary>
        private readonly Dictionary<TKey,int> _keyMap = new Dictionary<TKey, int>();

        /// <summary>
        ///     Query the keys
        /// </summary>
#if WINPHONE7
        public IEnumerable<TableKey<T, TKey>> Query { get { return _keyList; } }
#else
        public IQueryable<TableKey<T, TKey>> Query { get { return _keyList.AsQueryable(); } }
#endif

        private void _DeserializeKeys()
        {
            _keyList.Clear();
            _keyMap.Clear();

            var path = _pathProvider.GetKeysPath<T>(_databaseName);
            using (var iso = new IsoStorageHelper())
            {
                if (iso.FileExists(path))
                {
                    using (var br = iso.GetReader(path))
                    {
                        var count = br.ReadInt32();
                        for(var i = 0; i < count; i++)
                        {
                            var key = _serializer.Deserialize<TKey>(br);
                            var idx = br.ReadInt32();
                            _keyMap.Add(key, idx);
                            _keyList.Add(new TableKey<T, TKey>(key, _resolver));
                        }
                    }
                }
                else
                {
                    NextKey = 0;
                }
            }
        }

        /// <summary>
        ///     Serializes the key list
        /// </summary>
        private void _SerializeKeys()
        {
            using (var iso = new IsoStorageHelper())
            {
                iso.EnsureDirectory(_pathProvider.GetDatabasePath(_databaseName));
                iso.EnsureDirectory(_pathProvider.GetTablePath<T>(_databaseName));
                using (var bw = iso.GetWriter(_pathProvider.GetKeysPath<T>(_databaseName)))
                {
                    bw.Write(_keyMap.Count);
                    foreach(var key in _keyMap.Keys)
                    {
                        _serializer.Serialize(key, bw);
                        bw.Write(_keyMap[key]);
                    }
                }
            }
        }

        /// <summary>
        ///     The next key
        /// </summary>
        internal int NextKey { get; private set; } 

        /// <summary>
        ///     Serialize
        /// </summary>
        public void Flush()
        {
            lock (((ICollection)_keyList).SyncRoot)
            {
                if (IsDirty)
                {
                    _SerializeKeys();
                }
                IsDirty = false;             
            }
        }

        /// <summary>
        ///     Get the index for a key
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The index</returns>
        public int GetIndexForKey(object key)
        {
            return _keyMap.ContainsKey((TKey) key) ? _keyMap[(TKey) key] : -1; 
        }

        /// <summary>
        ///     Refresh the list
        /// </summary>
        public void Refresh()
        {
            lock (((ICollection)_keyList).SyncRoot)
            {
                if (IsDirty)
                {
                    _SerializeKeys();
                }
                _DeserializeKeys();
                IsDirty = false;               
            }
        }

        /// <summary>
        ///     Add a key to the list
        /// </summary>
        /// <param name="key">The key</param>
        public int AddKey(object key)
        {
            lock (((ICollection)_keyList).SyncRoot)
            {
                var newKey = new TableKey<T, TKey>((TKey) key, _resolver);

                if (!_keyList.Contains(newKey))
                {
                    _keyList.Add(newKey);
                    _keyMap.Add((TKey) key, NextKey++);
                    IsDirty = true;                   
                }
                else
                {
                    var idx = _keyList.IndexOf(newKey);
                    _keyList[idx].Refresh();
                }
            }

            return _keyMap[(TKey)key];
        }

        /// <summary>
        ///     Remove a key from the list
        /// </summary>
        /// <param name="key">The key</param>
        public void RemoveKey(object key)
        {
            lock (((ICollection)_keyList).SyncRoot)
            {
                var checkKey = new TableKey<T, TKey>((TKey) key, _resolver);

                if (!_keyList.Contains(checkKey)) return;
                _keyList.Remove(checkKey);
                _keyMap.Remove((TKey) key);
                IsDirty = true;             
            }
        }
    }
}
