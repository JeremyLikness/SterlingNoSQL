using System;
using Wintellect.Sterling.Database;
using Wintellect.Sterling.Serialization;

namespace Wintellect.Sterling
{
    /// <summary>
    ///     Sterling database interface
    /// </summary>
    public interface ISterlingDatabase : ISterlingLock 
    {
        /// <summary>
        ///     Registers a logger (multiple loggers may be registered)
        /// </summary>
        /// <param name="log">The call for logging</param>
        /// <returns>A unique identifier for the logger</returns>
        Guid RegisterLogger(Action<SterlingLogLevel, string, Exception> log);

        /// <summary>
        ///     Unhooks a logging mechanism
        /// </summary>
        /// <param name="guid">The guid</param>
        void UnhookLogger(Guid guid);
        
        /// <summary>
        ///     Log a message 
        /// </summary>
        /// <param name="level">The level</param>
        /// <param name="message">The message data</param>
        /// <param name="exception">The exception</param>
        void Log(SterlingLogLevel level, string message, Exception exception);

        /// <summary>
        ///     Register a database type with the system
        /// </summary>
        /// <typeparam name="T">The type of the database to register</typeparam>
        ISterlingDatabaseInstance RegisterDatabase<T>() where T : BaseDatabaseInstance;

        /// <summary>
        ///     Retrieve the database with the name
        /// </summary>
        /// <param name="databaseName">The database name</param>
        /// <returns>The database instance</returns>
        ISterlingDatabaseInstance GetDatabase(string databaseName);

        /// <summary>
        ///     Register a serializer with the system
        /// </summary>
        /// <typeparam name="T">The type of the serliaizer</typeparam>
        void RegisterSerializer<T>() where T : BaseSerializer;
    }
}