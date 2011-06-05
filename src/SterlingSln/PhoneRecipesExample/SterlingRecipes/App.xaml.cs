using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SterlingRecipes.Contracts;
using SterlingRecipes.Database;
using SterlingRecipes.Models;
using SterlingRecipes.ViewModels;
using UltraLight.MVVM;
using Wintellect.Sterling;
using Wintellect.Sterling.IsolatedStorage;

namespace SterlingRecipes
{
    public partial class App
    {
        #region Sterling Section

        private static SterlingEngine _engine;
        private static SterlingDefaultLogger _logger;

        /// <summary>
        ///     Reference to the main database engine
        /// </summary>
        public static ISterlingDatabaseInstance Database { get; private set; }

        /// <summary>
        ///     Use this is fire up the engine
        /// </summary>
        private static void _ActivateEngine()
        {
            _engine = new SterlingEngine();

            // custom serializer for types
            _engine.SterlingDatabase.RegisterSerializer<TypeSerializer>();

            // change this for more verbose messages
            _logger = new SterlingDefaultLogger(SterlingLogLevel.Information);
            _engine.Activate();

            // set it 
            Database = _engine.SterlingDatabase.RegisterDatabase<RecipeDatabase>(new IsolatedStorageDriver());

            // see if we need to load it 
            RecipeDatabase.CheckAndCreate(Database);
        }

        /// <summary>
        ///     This registers the view models we'll be using at runtime
        /// </summary>
        private static void _RegisterViewModels()
        {
            UltraLightLocator.Register<IMainViewModel>(new MainViewModel());
            UltraLightLocator.Register<IRecipeViewModel>(new RecipeViewModel());
            UltraLightLocator.Register<IIngredientViewModel>(new IngredientViewModel());
            UltraLightLocator.Register<ITextEditorViewModel>(new TextEditorViewModel());
        }

        /// <summary>
        ///     Use this to clear the database and flush it out
        /// </summary>
        private static void _DeactivateEngine()
        {
            Database.Flush();
            _logger.Detach();
            _engine.Dispose();
            Database = null;
            _engine = null;
        }

        #endregion

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += _ApplicationUnhandledException;

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters
                Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are being GPU accelerated with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;
            }

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            _InitializePhoneApplication();
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void _ApplicationLaunching(object sender, LaunchingEventArgs e)
        {
            _ActivateEngine();
            _RegisterViewModels();
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void _ApplicationActivated(object sender, ActivatedEventArgs e)
        {
            _ActivateEngine();
            _RegisterViewModels();
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void _ApplicationDeactivated(object sender, DeactivatedEventArgs e)
        {
            _DeactivateEngine();
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void _ApplicationClosing(object sender, ClosingEventArgs e)
        {
            Database.Truncate(typeof (TombstoneModel));
            _DeactivateEngine();
            // Ensure that required application state is persisted here.
        }

        // Code to execute if a navigation fails
        private static void _RootFrameNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private static void _ApplicationUnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool _phoneApplicationInitialized;

        static App()
        {
            Database = null;
        }

        // Do not add any additional code to this method
        private void _InitializePhoneApplication()
        {
            if (_phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += _CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += _RootFrameNavigationFailed;

            // Ensure we don't initialize again
            _phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void _CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= _CompleteInitializePhoneApplication;
        }

        #endregion
    }
}