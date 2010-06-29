using System.IO.IsolatedStorage;
using System.Windows;

namespace SterlingExample.Views
{
    /// <summary>
    ///     Request storage
    /// </summary>
    public partial class RequestStorage
    {
        const string HAPPY = "Thank you! The quota has been increased.";
        const string SAD = "The quota was not increased. The application may fail.";
            
        /// <summary>
        ///     Request storage
        /// </summary>
        public RequestStorage()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Click to request
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                MessageBox.Show(iso.IncreaseQuotaTo(SterlingService.QUOTA)
                                    ? HAPPY
                                    : SAD);
            }

            SterlingService.Current.Navigator.NavigateToBuildView();
        }
    }
}
