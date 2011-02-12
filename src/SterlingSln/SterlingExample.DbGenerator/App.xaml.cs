using System.Windows;

namespace SterlingExample.DbGenerator
{
    public partial class App
    {
        
        public App()
        {
            UnhandledException += Application_UnhandledException;
            InitializeComponent();
        }
        
        private static void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => ReportErrorToDOM(e));
        }

        private static void ReportErrorToDOM(ApplicationUnhandledExceptionEventArgs e)
        {
            var errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
            errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");
            MessageBox.Show(errorMsg);
        }
    }
}