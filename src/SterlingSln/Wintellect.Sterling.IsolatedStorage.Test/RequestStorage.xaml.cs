using System;
using System.IO.IsolatedStorage;
using System.Windows;

namespace Wintellect.Sterling.IsolatedStorage.Test
{
    public partial class RequestStorage
    {
        public Action Completed { get; set; }

        public const long QUOTA = 100*1024*1024;

        public RequestStorage()
        {
            InitializeComponent();
        }

        private void _ButtonClick(object sender, RoutedEventArgs e)
        {
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                iso.IncreaseQuotaTo(QUOTA);
                if (Completed != null)
                {
                    Completed();
                }
            }
        }
    }
}
