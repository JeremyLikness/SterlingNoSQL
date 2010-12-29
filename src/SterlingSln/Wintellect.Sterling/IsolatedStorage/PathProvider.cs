using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Wintellect.Sterling.Database;
using Wintellect.Sterling.Exceptions;

namespace Wintellect.Sterling.IsolatedStorage
{
    /// <summary>
    ///     Path provider
    /// </summary>
    internal class PathProvider
    {
        private readonly LogManager _logManager;

        internal const string BASE = "Sterling/";
        internal const string DB = "{0}db.dat";
        internal const string TYPE = "{0}types.dat";
        internal const string TABLE = "{0}tables.dat";
        internal const string KEY = "{0}keys.dat";

        /// <summary>
        ///     Next database index
        /// </summary>
        internal int NextDb { get; private set; }

        /// <summary>
        ///     Next table index
        /// </summary>
        internal int NextTable { get; private set; }

        /// <summary>
        ///     Master index of databases
        /// </summary>
        private readonly Dictionary<string, int> _databaseMaster = new Dictionary<string, int>();

        /// <summary>
        ///     Master list of types
        /// </summary>
        private readonly List<string> _typeMaster = new List<string>();

        /// <summary>
        ///     Master index of tables 
        /// </summary>
        private readonly Dictionary<int, Dictionary<Type, int>> _tableMaster =
            new Dictionary<int, Dictionary<Type, int>>();

        /// <summary>
        ///     Isolated storage
        /// </summary>
        private static readonly IsoStorageHelper _iso = new IsoStorageHelper();

        /// <summary>
        ///     Constructor
        /// </summary>
        public PathProvider(LogManager logManager)
        {
            _logManager = logManager;

            _iso.EnsureDirectory(BASE);

            _InitializeDatabases();
        }

        /// <summary>
        ///     Purges the database from the master list
        /// </summary>
        /// <param name="databaseName">The database name</param>
        public void Purge(string databaseName)
        {
            if (_databaseMaster.ContainsKey(databaseName))
            {
                lock (((ICollection) _databaseMaster).SyncRoot)
                {
                    var idx = _databaseMaster[databaseName];
                    _databaseMaster.Remove(databaseName);
                    lock (((ICollection) _tableMaster).SyncRoot)
                    {
                        if (_tableMaster.ContainsKey(idx))
                        {
                            _tableMaster.Remove(idx);
                        }
                    }
                }
            }
            _SerializeDatabases();
        }

        public int GetTypeIndex(string typeName)
        {
            if (_typeMaster.Count < 1)
            {
                _InitializeTypes();
            }

            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentNullException("typeName");
            }

            if (!_typeMaster.Contains(typeName))
            {
                lock(((ICollection)_typeMaster).SyncRoot)
                {
                    if (!_typeMaster.Contains(typeName))
                    {
                        _typeMaster.Add(typeName);
                        _SerializeTypes();
                    }
                }
            }

            return _typeMaster.IndexOf(typeName);
        }

        public string GetTypeAtIndex(int typeIndex)
        {
            if (_typeMaster.Count < 1)
            {
                _InitializeTypes();
            }

            if ((_typeMaster.Count - 1) < typeIndex)
            {
                throw new IndexOutOfRangeException("typeIndex");
            }

            return _typeMaster[typeIndex];
        }

        /// <summary>
        ///     Get the path for a database
        /// </summary>
        /// <param name="databaseName">The database name</param>
        /// <returns>The path</returns>
        public string GetDatabasePath(string databaseName)
        {
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new ArgumentNullException("databaseName");
            }

            _logManager.Log(SterlingLogLevel.Verbose,
                            string.Format("Path Provider: Database Path Request: {0}", databaseName), null);

            lock (((ICollection) _databaseMaster).SyncRoot)
            {
                if (!_databaseMaster.ContainsKey(databaseName))
                {
                    _databaseMaster.Add(databaseName, NextDb++);
                    _SerializeDatabases();
                    _tableMaster.Add(_databaseMaster[databaseName], new Dictionary<Type, int>());
                }
            }

            var path = string.Format("{0}{1}/", BASE, _databaseMaster[databaseName]);

            _logManager.Log(SterlingLogLevel.Verbose, string.Format("Resolved database path from {0} to {1}",
                                                                    databaseName, path), null);
            return path;
        }

        /// <summary>
        ///     Generic table path
        /// </summary>
        /// <typeparam name="T">Type of the table</typeparam>
        /// <param name="databaseName">The name of the database</param>
        /// <returns>The table path</returns>
        public string GetTablePath<T>(string databaseName) where T : class, new()
        {
            return GetTablePath(databaseName, typeof (T));
        }

        /// <summary>
        ///     Get the table path
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="tableType"></param>
        /// <returns></returns>
        public string GetTablePath(string databaseName, Type tableType)
        {
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new ArgumentNullException(databaseName);
            }

            if (!_databaseMaster.ContainsKey(databaseName))
            {
                throw new SterlingDatabaseNotFoundException(databaseName);
            }

            _logManager.Log(SterlingLogLevel.Verbose,
                            string.Format("Path Provider: Table Path Request: {0}", tableType.FullName), null);

            var tableRef = _tableMaster[_databaseMaster[databaseName]];

            lock (((ICollection) tableRef).SyncRoot)
            {
                if (tableRef.Count == 0)
                {
                    // see if there are table indices to load
                    _InitializeTable(string.Format(TABLE, GetDatabasePath(databaseName)), tableRef);
                }

                if (!tableRef.ContainsKey(tableType))
                {
                    tableRef.Add(tableType, NextTable++);
                    _SerializeDatabases();
                    _SerializeTables(databaseName);
                }
            }

            var path = string.Format("{0}{1}/", GetDatabasePath(databaseName), tableRef[tableType]);
            _logManager.Log(SterlingLogLevel.Verbose, string.Format("Resolved table path from {0} to {1}",
                                                                    tableType.FullName, path), null);
            return path;
        }

        /// <summary>
        ///     Gets the path to a specific instance
        /// </summary>
        /// <param name="databaseName">The database</param>
        /// <param name="tableType">The type of the table</param>
        /// <param name="keyIndex">The key index</param>
        /// <returns>The path</returns>
        public string GetInstancePath(string databaseName, Type tableType, int keyIndex)
        {
            return string.Format("{0}{1}", GetTablePath(databaseName, tableType), keyIndex);
        }

        /// <summary>
        ///     Generic keys path
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        public string GetKeysPath<T>(string databaseName) where T : class, new()
        {
            return GetKeysPath(databaseName, typeof (T));
        }

        /// <summary>
        ///     Get path to the keys
        /// </summary>
        /// <param name="databaseName">The name of the database</param>
        /// <param name="tableType">The type of the table</param>
        /// <returns>The path to the keys</returns>
        public string GetKeysPath(string databaseName, Type tableType)
        {
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new ArgumentNullException("databaseName");
            }

            var path = string.Format(KEY, GetTablePath(databaseName, tableType));

            _logManager.Log(SterlingLogLevel.Verbose, string.Format("Resolved key path for table {0} from to {1}",
                                                                    tableType.FullName, path), null);

            return path;
        }

        private void _SerializeTables(string databaseName)
        {
            var databaseIndex = _databaseMaster[databaseName];


            _iso.EnsureDirectory(GetDatabasePath(databaseName));
            var path = string.Format(TABLE, GetDatabasePath(databaseName));
            using (var bw = _iso.GetWriter(path))
            {
                var tableRef = _tableMaster[databaseIndex];
                bw.Write(tableRef.Count);
                var stringBuilder = new StringBuilder();
                foreach (var key in tableRef.Keys)
                {
                    bw.Write(key.AssemblyQualifiedName);
                    bw.Write(tableRef[key]);
                    stringBuilder.AppendFormat(" {0}={1} ", tableRef[key], key.AssemblyQualifiedName);
                }
                _logManager.Log(SterlingLogLevel.Information,
                                string.Format(
                                    "Sterling serialized {0} table definitions to path {1} for database {2}:{3}{4}",
                                    _tableMaster[databaseIndex].Count, path, databaseName, Environment.NewLine,
                                    stringBuilder), null);
            }
        }

        /// <summary>
        ///     Serialize the database master
        /// </summary>
        private void _SerializeDatabases()
        {
            _iso.EnsureDirectory(BASE);
            using (var bw = _iso.GetWriter(string.Format(DB, BASE)))
            {
                bw.Write(NextDb);
                bw.Write(NextTable);
                bw.Write(_databaseMaster.Count);
                foreach (var key in _databaseMaster.Keys)
                {
                    bw.Write(key);
                    bw.Write(_databaseMaster[key]);
                }
                _logManager.Log(SterlingLogLevel.Information,
                                string.Format(
                                    "Sterling serialized the master database nextDb={0} nextTable={1} databases={2}",
                                    NextDb, NextTable, _databaseMaster.Count), null);
            }            
        }

        private void _SerializeTypes()
        {
            _iso.EnsureDirectory(BASE);
            using (var bw = _iso.GetWriter(string.Format(TYPE, BASE)))
            {
                bw.Write(_typeMaster.Count);
                foreach(var type in _typeMaster)
                {
                    bw.Write(type);
                }
                _logManager.Log(SterlingLogLevel.Information,
                                string.Format(
                                    "Sterling serialized the master type list types={0}",
                                    _typeMaster.Count), null);
            }
        }

        /// <summary>
        ///     Initializes the database mappings 
        /// </summary>
        private void _InitializeDatabases()
        {
            var path = string.Format(DB, BASE);

            _logManager.Log(SterlingLogLevel.Verbose, string.Format("Initialize databases from path: {0}", path), null);


            if (!_iso.FileExists(path)) return;

            _databaseMaster.Clear();
            _tableMaster.Clear();

            using (var br = _iso.GetReader(path))
            {
                NextDb = br.ReadInt32();
                NextTable = br.ReadInt32();
                var count = br.ReadInt32();
                var stringBuilder = new StringBuilder();
                for (var i = 0; i < count; i++)
                {
                    var dbName = br.ReadString();
                    var dbIndex = br.ReadInt32();
                    stringBuilder.AppendFormat(" {0}={1} ", dbIndex, dbName);
                    _databaseMaster.Add(dbName, dbIndex);
                    _tableMaster.Add(dbIndex, new Dictionary<Type, int>());
                }
                _logManager.Log(SterlingLogLevel.Information,
                                string.Format(
                                    "Sterling de-serialized the master database nextDb={0} nextTable={1} databases={2}:{3}{4}",
                                    NextDb, NextTable, _databaseMaster.Count, Environment.NewLine, stringBuilder), null);
            }
        }

        /// <summary>
        ///     Initializes the database mappings 
        /// </summary>
        private void _InitializeTypes()
        {
            var path = string.Format(TYPE, BASE);
            _logManager.Log(SterlingLogLevel.Verbose, string.Format("Initialize types from path: {0}", path), null);
            if (!_iso.FileExists(path)) return;

            lock (((ICollection)_typeMaster).SyncRoot)
            {

                _typeMaster.Clear();

                using (var br = _iso.GetReader(path))
                {
                    var count = br.ReadInt32();
                    for (var i = 0; i < count; i++)
                    {
                        _typeMaster.Add(br.ReadString());
                    }
                    _logManager.Log(SterlingLogLevel.Information,
                                    string.Format(
                                        "Sterling de-serialized the type database types={0}",
                                        count), null);
                }
            }
        }

        /// <summary>
        ///     Initializes the database mappings 
        /// </summary>
        private void _InitializeTable(string path, IDictionary<Type, int> dictionaryRef)
        {
            _logManager.Log(SterlingLogLevel.Verbose, string.Format("Initialize tables from path: {0}", path), null);

            if (!_iso.FileExists(path)) return;

            using (var br = _iso.GetReader(path))
            {
                var count = br.ReadInt32();
                var stringBuilder = new StringBuilder();
                for (var i = 0; i < count; i++)
                {
                    var typeName = br.ReadString();
                    var tableIndex = br.ReadInt32();
                    stringBuilder.AppendFormat(" {0}={1} ", tableIndex, typeName);
                    dictionaryRef.Add(Type.GetType(typeName), tableIndex);
                }
                _logManager.Log(SterlingLogLevel.Information,
                                string.Format("Sterling de-serialized {0} table definitions from path {1}:{2}{3}",
                                              count, path, Environment.NewLine, stringBuilder), null);
            }
        }
    }
}