using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using SterlingExample.Database;
using Wintellect.Sterling;

namespace SterlingExample.DbGenerator
{
    /// <summary>
    ///     Sterling application service
    /// </summary>
    public sealed class SterlingService : IApplicationService, IApplicationLifetimeAware, IDisposable
    {
        public const long KILOBYTE = 1024;
        public const long MEGABYTE = 1024*KILOBYTE;
        public const long QUOTA = 100*MEGABYTE;

        private SterlingEngine _engine;
        
        private MainPage _mainPage;

        public UserControl MainVisual
        {
            get { return _mainPage.MainContent.Content as UserControl; }
            set { _mainPage.MainContent.Content = value;  }
        }

        public static SterlingService Current { get; private set; }

        public static void RequestBackup(BinaryWriter bw)
        {
            Current._engine.SterlingDatabase.Backup<FoodDatabase>(bw);
        }

        /// <summary>
        ///     Navigator
        /// </summary>
        public Navigation Navigator { get; private set; }

        public ISterlingDatabaseInstance Database { get; private set; }
        
        private SterlingDefaultLogger _logger; 

        /// <summary>
        /// Called by an application in order to initialize the application extension service.
        /// </summary>
        /// <param name="context">Provides information about the application state. </param>
        public void StartService(ApplicationServiceContext context)
        {
            if (DesignerProperties.IsInDesignTool) return;
            _engine = new SterlingEngine();            
            Current = this;
        }

        /// <summary>
        /// Called by an application in order to stop the application extension service. 
        /// </summary>
        public void StopService()
        {
            return;
        }

        /// <summary>
        /// Called by an application immediately before the <see cref="E:System.Windows.Application.Startup"/> event occurs.
        /// </summary>
        public void Starting()
        {
            if (DesignerProperties.IsInDesignTool) return;
            
            if (Debugger.IsAttached)
            {
                _logger = new SterlingDefaultLogger(SterlingLogLevel.Verbose);
            }

           // _engine.SterlingDatabase.RegisterSerializer<FoodSerializer>();

            _engine.Activate();
            Database = _engine.SterlingDatabase.RegisterDatabase<FoodDatabase>();

            _mainPage = new MainPage();
            Application.Current.RootVisual = _mainPage;
            Navigator = new Navigation(view=>MainVisual=view);
        }

        public static void ExecuteOnUIThread(Action action)
        {
            if (Deployment.Current.CheckAccess())
            {
                var dispatcher = Deployment.Current.Dispatcher;
                if (dispatcher.CheckAccess())
                {
                    dispatcher.BeginInvoke(action);
                }
            }
            else
            {
                action();
            }
        }

        /// <summary>
        /// Called by an application immediately after the <see cref="E:System.Windows.Application.Startup"/> event occurs.
        /// </summary>
        public void Started()
        {
            return;
        }

        /// <summary>
        /// Called by an application immediately before the <see cref="E:System.Windows.Application.Exit"/> event occurs. 
        /// </summary>
        public void Exiting()
        {
            if (DesignerProperties.IsInDesignTool) return;
            
            if (Debugger.IsAttached && _logger != null)
            {
                _logger.Detach();
            }
        }

        /// <summary>
        /// Called by an application immediately after the <see cref="E:System.Windows.Application.Exit"/> event occurs. 
        /// </summary>
        public void Exited()
        {
            Dispose();
            _engine = null;
            return;
        }

        public void RequestRebuild()
        {
            if (RebuildRequested != null)
            {
                RebuildRequested(this, EventArgs.Empty);
            }
        }

        public event EventHandler RebuildRequested;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_engine != null)
            {
                _engine.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}
