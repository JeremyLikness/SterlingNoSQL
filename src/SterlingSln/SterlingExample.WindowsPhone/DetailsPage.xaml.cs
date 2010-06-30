using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace SterlingExample.WindowsPhone
{
    public partial class DetailsPage : PhoneApplicationPage
    {
        public DetailsPage()
        {
            InitializeComponent();

            PageTransitionDetails.Completed += new EventHandler(PageTransitionDetails_Completed);

            SupportedOrientations = SupportedPageOrientation.Portrait | SupportedPageOrientation.Landscape;
        }

        // Handle navigating back to content in the two frames
        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Cancel default navigation
            e.Cancel = true;

            // Do page ransition animation
            PageTransitionDetails.Begin();
        }

        void PageTransitionDetails_Completed(object sender, EventArgs e)
        {
            // Reset root frame to MainPage.xaml
            PhoneApplicationFrame root = (PhoneApplicationFrame)Application.Current.RootVisual;
            root.GoBack();
        }
    }
}