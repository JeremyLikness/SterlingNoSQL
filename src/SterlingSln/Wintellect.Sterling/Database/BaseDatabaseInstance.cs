using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using Wintellect.Sterling.Events;
using Wintellect.Sterling.Exceptions;
using Wintellect.Sterling.Indexes;
using Wintellect.Sterling.IsolatedStorage;
using Wintellect.Sterling.Keys;
using Wintellect.Sterling.Serialization;

namespace Wintellect.Sterling.Database
{
    /// <summary>
    ///     Base class for a sterling database instance
    /// </summary>
    public abstract class BaseDatabaseInstance : ISterlingDatabaseInstance
    {
        /// <summary>
        ///     Master database locks
        /// </summary>
        private static readonly Dictionary<Type, object> _locks = new Dictionary<Type, object>();

        /// <summary>
        ///     The table definitions
        /// </summary>
        private readonly Dictionary<Type, ITableDefinition> _tableDefinitions = new Dictionary<Type, ITableDefinition>();

        /// <summary>
        ///     Path provider
        /// </summary>
        private readonly PathProvider _pathProvider;

        /// <summary>
        ///     Serializer
        /// </summary>
        internal ISterlingSerializer Serializer { get; set; }

        /// <summary>
        ///     Called when this should be deactivated
        /// </summary>
        internal static void Deactivate()
        {
            _locks.Clear();
        }

        private readonly IsoStorageHelper _iso = new IsoStorageHelper();

        /// <summary>
        ///     The base database instance
        /// </summary>
        protected BaseDatabaseInstance()
        {
            var registered = false;

            lock (((ICollection) _locks).SyncRoot)
            {
                if (!_locks.ContainsKey(GetType()))
                {
                    _locks.Add(GetType(), new object());
                }
                else
                {
                    registered = true;
                }
            }

            if (registered)
            {
                throw new SterlingDuplicateDatabaseException(this);
            }

            _pathProvider = SterlingFactory.GetPathProvider();
        }

        public void Unload()
        {
            Flush();
        }

        /// <summary>
        ///     Must return an object for synchronization
        /// </summary>
        public object Lock
        {
            get { return _locks[GetType()]; }
        }

        /// <summary>
        ///     The name of the database instance
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        ///     Method called from the constructor to register tables
        /// </summary>
        /// <returns>The list of tables for the database</returns>
        protected abstract List<ITableDefinition> _RegisterTables();

        /// <summary>
        ///     Returns a table definition 
        /// </summary>
        /// <typeparam name="T">The type of the table</typeparam>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <param name="keyFunction">The key mapping function</param>
        /// <returns>The table definition</returns>
        protected ITableDefinition CreateTableDefinition<T, TKey>(Func<T, TKey> keyFunction) where T : class, new()
        {
            return new TableDefinition<T, TKey>(_pathProvider, Name, Serializer,
                                                Load<T, TKey>, keyFunction);
        }

        /// <summary>
        ///     Call to publish tables 
        /// </summary>
        internal void PublishTables()
        {
            _iso.EnsureDirectory(_pathProvider.GetDatabasePath(Name));

            lock (((ICollection) _tableDefinitions).SyncRoot)
            {
                foreach (var table in _RegisterTables())
                {
                    _iso.EnsureDirectory(_pathProvider.GetTablePath(Name, table.TableType));
                    if (_tableDefinitions.ContainsKey(table.TableType))
                    {
                        throw new SterlingDuplicateTypeException(table.TableType, Name);
                    }
                    _tableDefinitions.Add(table.TableType, table);
                }
            }
        }

        /// <summary>
        ///     True if it is registered with the sterling engine
        /// </summary>
        /// <param name="instance">The instance</param>
        /// <returns>True if it can be persisted</returns>
        public bool IsRegistered<T>(T instance) where T : class
        {
            return IsRegistered(typeof (T));
        }

        /// <summary>
        ///     Non-generic registration check
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>True if it is registered</returns>
        public bool IsRegistered(Type type)
        {
            return _tableDefinitions.ContainsKey(type);
        }

        /// <summary>
        ///     Get the key for an object
        /// </summary>
        /// <param name="instance">The instance</param>
        /// <returns>The key</returns>
        public object GetKey(object instance)
        {
            if (!IsRegistered(instance.GetType()))
            {
                throw new SterlingTableNotFoundException(instance.GetType(), Name);
            }

            return _tableDefinitions[instance.GetType()].FetchKeyFromInstance(instance);
        }

        /// <summary>
        ///     Get the key for an object
        /// </summary>
        /// <param name="table">The instance type</param>
        /// <returns>The key type</returns>
        public Type GetKeyType(Type table)
        {
            if (!IsRegistered(table))
            {
                throw new SterlingTableNotFoundException(table, Name);
            }

            return _tableDefinitions[table].KeyType;
        }

        /// <summary>
        ///     Save it
        /// </summary>
        /// <typeparam name="T">The instance type</typeparam>
        /// <typeparam name="TKey">Save it</typeparam>
        /// <param name="instance">The instance</param>
        public TKey Save<T, TKey>(T instance) where T : class, new()
        {
            return (TKey) Save((object) instance);
        }

        /// <summary>
        ///     Query (keys only)
        /// </summary>
        /// <typeparam name="T">The type to query</typeparam>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <returns>The list of keys to query</returns>
#if WINPHONE7
        public IEnumerable<TableKey<T, TKey>> Query<T, TKey>() where T: class, new()
#else
        public IQueryable<TableKey<T, TKey>> Query<T, TKey>() where T : class, new()
#endif
        {
            if (!IsRegistered(typeof (T)))
            {
                throw new SterlingTableNotFoundException(typeof (T), Name);
            }

            return ((TableDefinition<T, TKey>) _tableDefinitions[typeof (T)]).KeyList.Query;
        }

        /// <summary>
        ///     Query an index
        /// </summary>
        /// <typeparam name="T">The table type</typeparam>
        /// <typeparam name="TIndex">The index type</typeparam>
        /// <typeparam name="TKey">The key type</typeparam>
        /// <param name="indexName">The name of the index</param>
        /// <returns>The indexed items</returns>
#if WINPHONE7
        public IEnumerable<TableIndex<T, TIndex, TKey>> Query<T, TIndex, TKey>(string indexName) where T : class, new()
#else
        public IQueryable<TableIndex<T, TIndex, TKey>> Query<T, TIndex, TKey>(string indexName) where T : class, new()
#endif
        {
            if (string.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException("indexName");
            }

            if (!IsRegistered(typeof (T)))
            {
                throw new SterlingTableNotFoundException(typeof (T), Name);
            }

            var tableDef = (TableDefinition<T, TKey>) _tableDefinitions[typeof (T)];

            if (!tableDef.Indexes.ContainsKey(indexName))
            {
                throw new SterlingIndexNotFoundException(indexName, typeof (T));
            }

            var collection = tableDef.Indexes[indexName] as IndexCollection<T, TIndex, TKey>;

            if (collection == null)
            {
                throw new SterlingIndexNotFoundException(indexName, typeof (T));
            }

            return collection.Query;
        }

        /// <summary>
        ///     Query an index
        /// </summary>
        /// <typeparam name="T">The table type</typeparam>
        /// <typeparam name="TIndex1">The first index type</typeparam>
        /// <typeparam name="TIndex2">The second index type</typeparam>
        /// <typeparam name="TKey">The key type</typeparam>
        /// <param name="indexName">The name of the index</param>
        /// <returns>The indexed items</returns>
#if WINPHONE7
        public IEnumerable<TableIndex<T, Tuple<TIndex1, TIndex2>, TKey>> Query<T, TIndex1, TIndex2, TKey>(string indexName)
            where T : class, new()
#else
        public IQueryable<TableIndex<T, Tuple<TIndex1, TIndex2>, TKey>> Query<T, TIndex1, TIndex2, TKey>(
            string indexName)
            where T : class, new()
#endif
        {
            if (string.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException("indexName");
            }

            if (!IsRegistered(typeof (T)))
            {
                throw new SterlingTableNotFoundException(typeof (T), Name);
            }

            var tableDef = (TableDefinition<T, TKey>) _tableDefinitions[typeof (T)];

            if (!tableDef.Indexes.ContainsKey(indexName))
            {
                throw new SterlingIndexNotFoundException(indexName, typeof (T));
            }

            var collection = tableDef.Indexes[indexName] as IndexCollection<T, TIndex1, TIndex2, TKey>;

            if (collection == null)
            {
                throw new SterlingIndexNotFoundException(indexName, typeof (T));
            }

            return collection.Query;
        }

        /// <summary>
        ///     Save when key is not known
        /// </summary>
        /// <param name="type">The type of the instance</param>
        /// <param name="instance">The instance</param>
        /// <returns>The key</returns>
        public object Save(Type type, object instance)
        {
            if (!_tableDefinitions.ContainsKey(type))
            {
                throw new SterlingTableNotFoundException(instance.GetType(), Name);
            }

            var key = _tableDefinitions[type].FetchKeyFromInstance(instance);
            var keyIndex = _tableDefinitions[type].Keys.AddKey(key);
            
            _iso.EnsureDirectory(_pathProvider.GetDatabasePath(Name));
            _iso.EnsureDirectory(_pathProvider.GetTablePath(Name, type));

            using (var memStream = new MemoryStream())
            {
                using (var bw = new BinaryWriter(memStream))
                {
                    var serializationHelper = new SerializationHelper(this, Serializer, SterlingFactory.GetLogger());
                    serializationHelper.Save(type, instance, bw);          
          
                    bw.Flush();
                    
                    memStream.Seek(0, SeekOrigin.Begin);

                    using (var isoWriter = _iso.GetWriter(_pathProvider.GetInstancePath(Name, type, keyIndex)))
                    {
                        isoWriter.Write(memStream.ToArray());
                    }
                }                                                                          
            }            

            // update the indexes
            foreach (var index in _tableDefinitions[type].Indexes.Values)
            {
                index.AddIndex(instance, key);
            }

            _RaiseOperation(SterlingOperation.Save, type, key);

            return key;
        }

        /// <summary>
        ///     Save when key is not known
        /// </summary>
        /// <typeparam name="T">The type of the instance</typeparam>
        /// <param name="instance">The instance</param>
        /// <returns>The key</returns>
        public object Save<T>(T instance) where T : class, new()
        {
            return Save(typeof (T), instance);
        }

        /// <summary>
        ///     Save asynchronously
        /// </summary>
        /// <typeparam name="T">The type to save</typeparam>
        /// <param name="list">A list of items to save</param>
        /// <returns>A unique identifier for the batch</returns>
        public BackgroundWorker SaveAsync<T>(IList<T> list)
        {
            return SaveAsync((IList) list);
        }

        /// <summary>
        ///     Non-generic asynchronous save
        /// </summary>
        /// <param name="list">The list of items</param>
        /// <returns>A unique job identifier</returns>
        public BackgroundWorker SaveAsync(IList list)
        {
            if (list == null)
            {
                throw new ArgumentNullException("list");
            }

            if (list.Count == 0)
            {
                return null;
            }

            if (!IsRegistered(list[0].GetType()))
            {
                throw new SterlingTableNotFoundException(list[0].GetType(), Name);
            }

            var bw = new BackgroundWorker();

            bw.DoWork += (o, e) =>
                             {
                                 var index = 0;
                                 foreach (var item in list)
                                 {
                                     if (bw.CancellationPending)
                                     {
                                         e.Cancel = true;
                                         break;
                                     }

                                     Save(item.GetType(), item);

                                     var pct = index++*100/list.Count;
                                     bw.ReportProgress(pct);
                                 }
                             };

            return bw;
        }

        /// <summary>
        ///     Flush all keys and indexes to storage
        /// </summary>
        public void Flush()
        {
            if (_locks == null || !_locks.ContainsKey(GetType())) return;

            lock (Lock)
            {
                foreach (var def in _tableDefinitions.Values)
                {
                    def.Keys.Flush();

                    foreach (var idx in def.Indexes.Values)
                    {
                        idx.Flush();
                    }
                }
            }

            _RaiseOperation(SterlingOperation.Flush, GetType(), Name);
        }

        /// <summary>
        ///     Load it 
        /// </summary>
        /// <typeparam name="T">The type to load</typeparam>
        /// <typeparam name="TKey">The key type</typeparam>
        /// <param name="key">The value of the key</param>
        /// <returns>The instance</returns>
        public T Load<T, TKey>(TKey key) where T : class, new()
        {
            return (T) Load(typeof (T), key);
        }

        /// <summary>
        ///     Load it (key type not typed)
        /// </summary>
        /// <typeparam name="T">The type to load</typeparam>
        /// <param name="key">The key</param>
        /// <returns>The instance</returns>
        public T Load<T>(object key) where T : class, new()
        {
            return (T) Load(typeof (T), key);
        }

        /// <summary>
        ///     Load it without knowledge of the key type
        /// </summary>
        /// <param name="type">The type to load</param>
        /// <param name="key">The key</param>
        /// <returns>The instance</returns>
        public object Load(Type type, object key)
        {
            if (!_tableDefinitions.ContainsKey(type))
            {
                throw new SterlingTableNotFoundException(type, Name);
            }


            var keyIndex = _tableDefinitions[type].Keys.GetIndexForKey(key);

            // key not found
            if (keyIndex < 0)
            {
                return null;
            }

            object obj;

            using (var br = _iso.GetReader(_pathProvider.GetInstancePath(Name, type, keyIndex)))
            {
                var serializationHelper = new SerializationHelper(this, Serializer, SterlingFactory.GetLogger());
                obj = serializationHelper.Load(type, br);
            }

            _RaiseOperation(SterlingOperation.Load, type, key);
            return obj;
        }

        /// <summary>
        ///     Delete it 
        /// </summary>
        /// <typeparam name="T">The type to delete</typeparam>
        /// <param name="instance">The instance</param>
        public void Delete<T>(T instance) where T : class
        {
            Delete(typeof (T), _tableDefinitions[typeof (T)].FetchKeyFromInstance(instance));
        }

        /// <summary>
        ///     Delete it (non-generic)
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="key">The key</param>
        public void Delete(Type type, object key)
        {
            if (!_tableDefinitions.ContainsKey(type))
            {
                throw new SterlingTableNotFoundException(type, Name);
            }

            var keyEntry = _tableDefinitions[type].Keys.GetIndexForKey(key);


            _iso.Delete(_pathProvider.GetInstancePath(Name, type, keyEntry));


            _tableDefinitions[type].Keys.RemoveKey(key);
            foreach (var index in _tableDefinitions[type].Indexes.Values)
            {
                index.RemoveIndex(key);
            }

            _RaiseOperation(SterlingOperation.Delete, type, key);
        }

        /// <summary>
        ///     Truncate all records for a type
        /// </summary>
        /// <param name="type">The type</param>
        public void Truncate(Type type)
        {
            _iso.Purge(_pathProvider.GetTablePath(Name, type));


            _RaiseOperation(SterlingOperation.Truncate, type, null);
        }

        /// <summary>
        ///     Purge the entire database - wipe it clean!
        /// </summary>
        public void Purge()
        {
            _iso.Purge(_pathProvider.GetDatabasePath(Name));

            _pathProvider.Purge(Name);

            _RaiseOperation(SterlingOperation.Purge, GetType(), Name);
        }

        /// <summary>
        ///     Raise an operation
        /// </summary>
        /// <remarks>
        ///     Only send if access to the UI thread is available
        /// </remarks>
        /// <param name="operation">The operation</param>
        /// <param name="targetType">Target type</param>
        /// <param name="key">Key</param>
        private void _RaiseOperation(SterlingOperation operation, Type targetType, object key)
        {
            var handler = SterlingOperationPerformed;

            if (handler == null) return;

            Action raise = () => handler(this, new SterlingOperationArgs(operation, targetType, key));

            var dispatcher = Deployment.Current.Dispatcher;
            if (dispatcher.CheckAccess())
            {
                raise();
            }
            else
            {
                dispatcher.BeginInvoke(raise);
            }
        }

        public event EventHandler<SterlingOperationArgs> SterlingOperationPerformed;
    }
}