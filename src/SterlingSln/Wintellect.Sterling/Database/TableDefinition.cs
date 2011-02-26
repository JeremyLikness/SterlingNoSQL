using System;
using System.Collections.Generic;
using Wintellect.Sterling.Exceptions;
using Wintellect.Sterling.Indexes;
using Wintellect.Sterling.IsolatedStorage;
using Wintellect.Sterling.Keys;
using Wintellect.Sterling.Serialization;

namespace Wintellect.Sterling.Database
{
    /// <summary>
    ///     The definition of a table
    /// </summary>
    internal class TableDefinition<T,TKey> : ITableDefinition where T: class, new()
    {
        private readonly PathProvider _pathProvider;
        private readonly Func<TKey, T> _resolver;
        private Predicate<T> _isDirty; 
        private readonly ISterlingSerializer _serializer;
        private readonly string _databaseName;
           
        /// <summary>
        ///     Construct 
        /// </summary>
        /// <param name="pathProvider">Path provider</param>
        /// <param name="databaseName">The name of the database</param>
        /// <param name="serializer">The serializer provider</param>
        /// <param name="resolver">The resolver for the instance</param>
        /// <param name="key">The resolver for the key</param>
        public TableDefinition(PathProvider pathProvider, string databaseName, ISterlingSerializer serializer, Func<TKey,T> resolver, Func<T,TKey> key)
        {
            FetchKey = key;
            _pathProvider = pathProvider;
            _resolver = resolver;
            _serializer = serializer;
            _databaseName = databaseName;
            _isDirty = obj => true;
            KeyList = new KeyCollection<T, TKey>(pathProvider, databaseName, serializer, resolver);
            Indexes = new Dictionary<string, IIndexCollection>();
        }

        /// <summary>
        ///     Function to fetch the key
        /// </summary>
        public Func<T, TKey> FetchKey { get; private set; }

        /// <summary>
        ///     The key list
        /// </summary>
        public KeyCollection<T, TKey> KeyList { get; private set; }

        /// <summary>
        ///     The index list
        /// </summary>
        public Dictionary<string, IIndexCollection> Indexes { get; private set; }

        public void RegisterDirtyFlag(Predicate<T> isDirty)
        {
            _isDirty = isDirty;
        }

        /// <summary>
        ///     Registers an index with the table definition
        /// </summary>
        /// <typeparam name="TIndex">The type of the index</typeparam>
        /// <param name="name">A name for the index</param>
        /// <param name="indexer">The function to retrieve the index</param>
        public void RegisterIndex<TIndex>(string name, Func<T,TIndex> indexer)
        {
            if (Indexes.ContainsKey(name))
            {
                throw new SterlingDuplicateIndexException(name, typeof(T), _databaseName);
            }

            var indexCollection = new IndexCollection<T, TIndex, TKey>(name, _pathProvider,
                _databaseName, _serializer, indexer, _resolver);
            
            Indexes.Add(name, indexCollection);
        }

        /// <summary>
        ///     Registers an index with the table definition
        /// </summary>
        /// <typeparam name="TIndex1">The type of the first index</typeparam>
        /// <typeparam name="TIndex2">The type of the second index</typeparam>        
        /// <param name="name">A name for the index</param>
        /// <param name="indexer">The function to retrieve the index</param>
        public void RegisterIndex<TIndex1,TIndex2>(string name, Func<T, Tuple<TIndex1,TIndex2>> indexer)
        {
            if (Indexes.ContainsKey(name))
            {
                throw new SterlingDuplicateIndexException(name, typeof(T), _databaseName);
            }

            var indexCollection = new IndexCollection<T, TIndex1, TIndex2, TKey>(name, _pathProvider,
                _databaseName, _serializer, indexer, _resolver);

            Indexes.Add(name, indexCollection);
        }

        /// <summary>
        ///     Key list
        /// </summary>
        public IKeyCollection Keys { get { return KeyList; }}
        
        /// <summary>
        ///     Table type
        /// </summary>
        public Type TableType
        {
            get { return typeof(T); }
        }

        /// <summary>
        ///     Key type
        /// </summary>
        public Type KeyType
        {
            get { return typeof (TKey); }
        }

        /// <summary>
        ///     Refresh key list
        /// </summary>
        public void Refresh()
        {
            KeyList.Refresh();
            
            foreach(var index in Indexes.Values)
            {
                index.Refresh();               
            }
        }

        /// <summary>
        ///     Fetch the key for the instance
        /// </summary>
        /// <param name="instance">The instance</param>
        /// <returns>The key</returns>
        public object FetchKeyFromInstance(object instance)
        {
            return FetchKey((T) instance);
        }

        /// <summary>
        ///     Is the instance dirty?
        /// </summary>
        /// <returns>True if dirty</returns>
        public bool IsDirty(object instance)
        {
            return _isDirty((T) instance);
        }
    }
}
