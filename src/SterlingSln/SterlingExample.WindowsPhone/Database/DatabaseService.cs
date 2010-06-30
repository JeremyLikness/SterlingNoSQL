using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Wintellect.Sterling;

namespace SterlingExample.WindowsPhone.Database
{
    /// <summary>
    ///     Database service - Sterling 
    /// </summary>
    public class DatabaseService : IApplicationService, IApplicationLifetimeAware
    {
        /// <summary>
        ///     Current service
        /// </summary>
        public static DatabaseService Current { get; private set; }

        /// <summary>
        ///     Database
        /// </summary>
        public ISterlingDatabaseInstance Database { get; private set; }

        private SterlingEngine _engine;
        private SterlingDefaultLogger _logger; 

        public void StartService(ApplicationServiceContext context)
        {
            
            Current = this;
            _engine = new SterlingEngine();
            _logger = new SterlingDefaultLogger(SterlingLogLevel.Verbose);
        }

        public void StopService()
        {
            _engine = null;
        }

        public void Starting()
        {
            _engine.Activate();
            Database = _engine.SterlingDatabase.RegisterDatabase<PhoneDatabase>();
        }

        public void Started()
        {
            return;
        }

        public void Exiting()
        {
            _logger.Detach();
            _engine.Dispose();
        }

        public void Exited()
        {
            return;
        }
    }
}
