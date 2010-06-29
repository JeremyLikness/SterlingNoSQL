using Microsoft.Silverlight.Testing;

namespace Wintellect.Sterling.Test
{
    /// <summary>
    /// Unit tests for System.Windows.Controls.
    /// </summary>
    public partial class App
    {
        /// <summary>
        ///     Default logger for sterling (debugger) 
        /// </summary>
        private SterlingDefaultLogger _logger;

        /// <summary>
        /// Initializes a new instance of the App class.
        /// </summary>
        public App()
        {
            Startup += delegate
            {
                _logger = new SterlingDefaultLogger(SterlingLogLevel.Verbose);
                var settings = UnitTestSystem.CreateDefaultSettings();
                RootVisual = UnitTestSystem.CreateTestPage(settings);
            };

            InitializeComponent();

            Exit += (o, e) => _logger.Detach();
        }
    }
}
