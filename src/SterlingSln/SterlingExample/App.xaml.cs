using System;
using System.ComponentModel;
using System.Windows;
using SterlingExample.Database;
using Wintellect.Sterling;

namespace SterlingExample
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