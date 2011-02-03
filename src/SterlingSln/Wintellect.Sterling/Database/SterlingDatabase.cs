using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wintellect.Sterling.Exceptions;
using Wintellect.Sterling.Serialization;

namespace Wintellect.Sterling.Database
{
    /// <summary>
    ///     The sterling database manager
    /// </summary>
    internal class SterlingDatabase : ISterlingDatabase
    {        
        private static bool _activated;

        /// <summary>
        ///     Master list of databases
        /// </summary>
        private readonly Dictionary<string,Tuple<Type,ISterlingDatabaseInstance>> _databases = new Dictionary<string,Tuple<Type,ISterlingDatabaseInstance>>();

        /// <summary>
        ///     Backup manager
        /// </summary>
        private readonly BackupManager _backupManager = new BackupManager();        

        /// <summary>
        ///     The main serializer
        /// </summary>
        private ISterlingSerializer _serializer = new AggregateSerializer();

        /// <summary>
        ///     Logger
        /// </summary>
        private readonly LogManager _logManager;

        internal SterlingDatabase(LogManager logger)
        {
            _logManager = logger;
        }
        
        /// <summary>
        ///     Registers a logger (multiple loggers may be registered)
        /// </summary>
        /// <param name="log">The call for logging</param>
        /// <returns>A unique identifier for the logger</returns>
        public Guid RegisterLogger(Action<SterlingLogLevel, string, Exception> log)
        {
            return _logManager.RegisterLogger(log);
        }

        /// <summary>
        ///     Unhooks a logging mechanism
        /// </summary>
        /// <param name="guid">The guid</param>
        public void UnhookLogger(Guid guid)
        {
            _logManager.UnhookLogger(guid);
        }

        /// <summary>
        ///     Log a message 
        /// </summary>
        /// <param name="level">The level</param>
        /// <param name="message">The message data</param>
        /// <param name="exception">The exception</param>
        public void Log(SterlingLogLevel level, string message, Exception exception)
        {
            _logManager.Log(level, message, exception);
        }

        /// <summary>
        ///     Back up the database
        /// </summary>
        /// <typeparam name="T">The database type</typeparam>
        /// <param name="writer">A writer to receive the backup</param>
        public void Backup<T>(BinaryWriter writer) where T : BaseDatabaseInstance
        {
            _RequiresActivation();

            var databaseQuery = from d in _databases where d.Value.Item1.Equals(typeof (T)) select d.Value.Item2;
            if (!databaseQuery.Any())
            {
                throw new SterlingDatabaseNotFoundException(typeof(T).FullName);
            }
            var database = databaseQuery.First();
            database.Flush();
            var path = ((BaseDatabaseInstance) database).Path;
            _backupManager.Backup(writer, path);
        }

        /// <summary>
        ///     Restore the database
        /// </summary>
        /// <typeparam name="T">Type of the database</typeparam>
        /// <param name="reader">The reader with the backup information</param>
        public void Restore<T>(BinaryReader reader) where T : BaseDatabaseInstance
        {
            _RequiresActivation();

            var databaseQuery = from d in _databases where d.Value.Item1.Equals(typeof(T)) select d.Value.Item2;
            if (!databaseQuery.Any())
            {
                throw new SterlingDatabaseNotFoundException(typeof(T).FullName);
            }
            var database = databaseQuery.First();
            var path = ((BaseDatabaseInstance)database).Path;
            database.Purge();
            _backupManager.Restore(reader, path);
        }

        /// <summary>
        ///     Register a database type with the system
        /// </summary>
        /// <typeparam name="T">The type of the database to register</typeparam>
        public ISterlingDatabaseInstance RegisterDatabase<T>() where T : BaseDatabaseInstance
        {
            _RequiresActivation();
            _logManager.Log(SterlingLogLevel.Information, 
                string.Format("Sterling is registering database {0}", typeof(T)),
                null);  
            
            if ((from d in _databases where d.Value.Item1.Equals(typeof(T)) select d).Count() > 0)
            {
                throw new SterlingDuplicateDatabaseException(typeof(T));
            }
            
            var database = (ISterlingDatabaseInstance)Activator.CreateInstance(typeof (T));
            
            ((BaseDatabaseInstance) database).Serializer = _serializer;          
            
            ((BaseDatabaseInstance)database).PublishTables();
            _databases.Add(database.Name, new Tuple<Type, ISterlingDatabaseInstance>(typeof(T),database));
            return database;
        }

        /// <summary>
        ///     Unloads/flushes the database instances
        /// </summary>
        private void _Unload()
        {
            foreach (var database in
                _databases.Values.Select(databaseDef => databaseDef.Item2).OfType<BaseDatabaseInstance>())
            {
                database.Unload();
            }
        }

        /// <summary>
        ///     Retrieve the database with the name
        /// </summary>
        /// <param name="databaseName">The database name</param>
        /// <returns>The database instance</returns>
        public ISterlingDatabaseInstance GetDatabase(string databaseName)
        {
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new ArgumentNullException("databaseName");
            }
            
            _RequiresActivation();
            
            if (!_databases.ContainsKey(databaseName))
            {
                throw new SterlingDatabaseNotFoundException(databaseName);
            }
            
            return _databases[databaseName].Item2;
        }

        /// <summary>
        ///     Register a serializer with the system
        /// </summary>
        /// <typeparam name="T">The type of the serliaizer</typeparam>
        public void RegisterSerializer<T>() where T : BaseSerializer
        {
            if (_activated)
            {
                throw new SterlingActivationException(string.Format("RegisterSerializer<{0}>", typeof(T).FullName));
            }

            ((AggregateSerializer)_serializer).AddSerializer((ISterlingSerializer)Activator.CreateInstance(typeof(T)));
        }

        /// <summary>
        ///     Must be called to activate the engine. 
        ///     Can only be called once.
        /// </summary>
        public void Activate()
        {
            lock(Lock)
            {
                if (_activated)
                {
                    throw new SterlingActivationException("Activate()");
                }
                _LoadDefaultSerializers();
                _activated = true;                
            }
        }

        private void _LoadDefaultSerializers()
        {
            // Load default serializes
            RegisterSerializer<DefaultSerializer>();  
            RegisterSerializer<ExtendedSerializer>();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Deactivate()
        {
            lock(Lock)
            {
                _activated = false;
                SterlingFactory.GetPathProvider().Serialize();
                _Unload();
                _databases.Clear();
                BaseDatabaseInstance.Deactivate();
                _serializer = new AggregateSerializer();
            }

            return;
        }

        /// <summary>
        ///     Requires that sterling is activated
        /// </summary>
        private static void _RequiresActivation()
        {
            if (!_activated)
            {
                throw new SterlingNotReadyException();
            }
        }

        private static readonly object _lock = new object();

        public object Lock
        {
            get { return _lock; }
        }
    }
}
