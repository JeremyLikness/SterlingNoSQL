using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace SterlingExample.DbGenerator.Views
{
    public partial class DownloadView
    {
        private SaveFileDialog _saveDialog; 

        public DownloadView()
        {
            InitializeComponent();

            _saveDialog = new SaveFileDialog
                              {
                                  DefaultExt = ".sdb",
                                  Filter = "Sterling Database Files|*.sdb|All Files|*.*",
                                  FilterIndex = 1
                              };
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            var dialogResult = _saveDialog.ShowDialog();

            if (dialogResult == true)
            {
                using (var stream = _saveDialog.OpenFile())
                {
                    using (var bw = new BinaryWriter(stream))
                    {
                        SterlingService.RequestBackup(bw);
                    }
                }
            }   

        }
    }
}
