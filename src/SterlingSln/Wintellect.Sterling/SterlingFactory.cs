using Wintellect.Sterling.Database;
using Wintellect.Sterling.IsolatedStorage;

namespace Wintellect.Sterling
{
    /// <summary>
    ///     Factory to retrieve the sterling manager
    /// </summary>
    internal static class SterlingFactory
    {
        /// <summary>
        ///     Instance of the database
        /// </summary>
        private static ISterlingDatabase _database; 

        /// <summary>
        ///     The log manager
        /// </summary>
        private static LogManager _logManager;

        /// <summary>
        ///     Path provider
        /// </summary>
        private static PathProvider _pathProvider;

        static SterlingFactory()
        {
            Initialize();
        }

        internal static void Initialize()
        {
            _logManager = new LogManager();
            _pathProvider = new PathProvider(_logManager);
            _database = new SterlingDatabase(_logManager);
        }

        /// <summary>
        ///     Gets the database engine
        /// </summary>
        /// <returns>The instance of the database engine</returns>
        public static ISterlingDatabase GetDatabaseEngine()
        {
            return _database;
        }

        /// <summary>
        ///     Logger
        /// </summary>
        /// <returns>The logger</returns>
        internal static LogManager GetLogger()
        {
            return _logManager;
        }

        /// <summary>
        ///     Path provider
        /// </summary>
        /// <returns>The path provider</returns>
        internal static PathProvider GetPathProvider()
        {
            return _pathProvider;
        }
    }
}
