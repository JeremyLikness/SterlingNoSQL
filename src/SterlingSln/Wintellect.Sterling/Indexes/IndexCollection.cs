using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wintellect.Sterling.Exceptions;
using Wintellect.Sterling.IsolatedStorage;
using Wintellect.Sterling.Serialization;

namespace Wintellect.Sterling.Indexes
{
    /// <summary>
    ///     Collection of keys for a given entity
    /// </summary>
    internal class IndexCollection<T, TIndex, TKey> : IIndexCollection where T : class, new()
    {
        private PathProvider _pathProvider;
        private string _databaseName;
        protected Func<TKey, T> Resolver;
        private Func<T, TIndex> _indexer;
        private ISterlingSerializer _serializer;
        private string _name;
        
        /// <summary>
        ///     Help with serializing tuples
        /// </summary>
        protected Action<TIndex,BinaryWriter,ISterlingSerializer> SerializeTuple { get; private set;}

        /// <summary>
        ///     Help with deserializing tuples
        /// </summary>
        protected Func<ISterlingSerializer, BinaryReader, TIndex> DeserializeTuple { get; private set; }

        /// <summary>
        ///     True if it is a tuple
        /// </summary>
        protected bool IsTuple { get; set; }

        /// <summary>
        ///     Set when keys change
        /// </summary>
        public bool IsDirty { get; private set; }

        /// <summary>
        ///     Initialize the key collection
        /// </summary>
        /// <param name="name">name of the index</param>
        /// <param name="pathProvider">Path provider</param>
        /// <param name="databaseName">Name of the database</param>
        /// <param name="serializer">The serializer it can use to write/restore keys</param>
        /// <param name="indexer">How to resolve the index</param>
        /// <param name="resolver">The resolver for loading the object</param>
        public IndexCollection(string name, 
            PathProvider pathProvider, string databaseName, ISterlingSerializer serializer, Func<T,TIndex> indexer, Func<TKey,T> resolver)
        {
            if (!serializer.CanSerialize<TIndex>())
            {
                throw new SterlingSerializerException(serializer, typeof(TIndex));
            }

            _Setup(name, indexer, pathProvider, resolver, databaseName, serializer);
        }

        /// <summary>
        ///     Constructor with a tuple on the surface
        /// </summary>
        /// <param name="name">Name of the index</param>
        /// <param name="pathProvider">Path provider</param>
        /// <param name="databaseName">Name of the database</param>
        /// <param name="serializer">The serializer it can use to write/restore keys</param>
        /// <param name="indexer">How to resolve the index</param>
        /// <param name="resolver">The resolver for loading the object</param>
        /// <param name="serializeTuple">Method to serialize the individual tuple parts</param>
        /// <param name="deserializeTuple">Method to deserialize the individual tuple parts</param>
        public IndexCollection(string name,
            PathProvider pathProvider, string databaseName, ISterlingSerializer serializer, Func<T, TIndex> indexer, Func<TKey, T> resolver,
            Action<TIndex,BinaryWriter,ISterlingSerializer> serializeTuple, Func<ISterlingSerializer, BinaryReader, TIndex> deserializeTuple)
        {
            IsTuple = true;
            SerializeTuple = serializeTuple;
            DeserializeTuple = deserializeTuple;      

            _Setup(name, indexer, pathProvider, resolver, databaseName, serializer);            
        }

        /// <summary>
        ///     Common constructor calls
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pathProvider">Path provider</param>
        /// <param name="databaseName">Name of the database</param>
        /// <param name="serializer">The serializer it can use to write/restore keys</param>
        /// <param name="indexer">How to resolve the index</param>
        /// <param name="resolver">The resolver for loading the object</param>
        private void _Setup(string name, Func<T, TIndex> indexer, PathProvider pathProvider, Func<TKey, T> resolver, string databaseName, ISterlingSerializer serializer)
        {
            _name = name;
            _indexer = indexer;
            _pathProvider = pathProvider;
            Resolver = resolver;
            _databaseName = databaseName;
            _serializer = serializer;
            
            _DeserializeIndexes();

            IsDirty = false;
        }


        /// <summary>
        ///     The list of indexes
        /// </summary>
        private readonly List<TableIndex<T, TIndex, TKey>> _indexList = new List<TableIndex<T, TIndex, TKey>>();
        
        /// <summary>
        ///     Query the indexes
        /// </summary>
#if WINPHONE7
        public IEnumerable<TableIndex<T, TIndex, TKey>> Query { get { return _indexList; } }
#else
        public IQueryable<TableIndex<T, TIndex, TKey>> Query { get { return _indexList.AsQueryable(); } }
#endif

        /// <summary>
        ///     Deserialize the indexes
        /// </summary>
        private void _DeserializeIndexes()
        {
            _indexList.Clear();            
                        
            using (var iso = new IsoStorageHelper())
            {
                if (!iso.FileExists(_IndexTable())) return;
                using (var br = iso.GetReader(_IndexTable()))
                {
                    var count = br.ReadInt32();
                    for (var i = 0; i < count; i++)
                    {
                        var index =
                            IsTuple ? DeserializeTuple(_serializer, br) :
                                                                            _serializer.Deserialize<TIndex>(br);
                        var key = _serializer.Deserialize<TKey>(br);
                        _indexList.Add(new TableIndex<T, TIndex, TKey>(index, key, Resolver));
                    }
                }
            }
        }

        /// <summary>
        ///     Serializes the key list
        /// </summary>
        private void _SerializeIndexes()
        {
            using (var iso = new IsoStorageHelper())
            {
                iso.EnsureDirectory(_pathProvider.GetDatabasePath(_databaseName));
                iso.EnsureDirectory(_pathProvider.GetTablePath<T>(_databaseName));
                using (var bw = iso.GetWriter(_IndexTable()))
                {
                    bw.Write(_indexList.Count);
                    foreach (var index in _indexList)
                    {
                        if (IsTuple)
                        {
                            SerializeTuple(index.Index, bw, _serializer);
                        }
                        else
                        {
                            _serializer.Serialize(index.Index, bw);
                        }
                        _serializer.Serialize(index.Key, bw);                        
                    }
                }
            }
        }
        
        /// <summary>
        ///     Serialize
        /// </summary>
        public void Flush()
        {
            lock (((ICollection)_indexList).SyncRoot)
            {
                if (IsDirty)
                {
                    _SerializeIndexes();
                }
                IsDirty = false;
            }
        }             

        /// <summary>
        ///     Refresh the list
        /// </summary>
        public void Refresh()
        {
            lock (((ICollection)_indexList).SyncRoot)
            {
                if (IsDirty)
                {
                    _SerializeIndexes();
                }
                _DeserializeIndexes();
                IsDirty = false;
            }
        }       

        /// <summary>
        ///     Add an index to the list
        /// </summary>
        /// <param name="instance">The instance</param>
        /// <param name="key">The related key</param>
        public void AddIndex(object instance, object key)
        {
            var newIndex = new TableIndex<T, TIndex, TKey>(_indexer((T)instance), (TKey)key, Resolver);
            lock(((ICollection)_indexList).SyncRoot)
            {
                if (!_indexList.Contains(newIndex))
                {
                    _indexList.Add(newIndex);
                }
                else
                {
                    _indexList[_indexList.IndexOf(newIndex)] = newIndex;
                }
            }
            IsDirty = true;
        }

        /// <summary>
        ///     Update the index
        /// </summary>
        /// <param name="instance">The instance</param>
        /// <param name="key">The key</param>
        public void UpdateIndex(object instance, object key)
        {
            var index = (from i in _indexList where i.Key.Equals(key) select i).FirstOrDefault();

            if (index == null) return;

            index.Index = _indexer((T)instance);
            index.Refresh();
            IsDirty = true;
        }

        /// <summary>
        ///     Remove an index from the list
        /// </summary>
        /// <param name="key">The key</param>
        public void RemoveIndex(object key)
        {
            var index = (from i in _indexList where i.Key.Equals(key) select i).FirstOrDefault();

            if (index == null) return;
            
            lock(((ICollection)_indexList).SyncRoot)
            {
                if (!_indexList.Contains(index)) return;

                _indexList.Remove(index);
                IsDirty = true;
            }
        }

        /// <summary>
        ///     Get the path to the index
        /// </summary>
        /// <returns>The path to the index</returns>
        private string _IndexTable()
        {
            var tablePath = _pathProvider.GetTablePath<T>(_databaseName);
            return string.Format("{0}{1}.idx", tablePath, _name);
        }
    }
}
