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
using System.Windows.Navigation;
using SterlingExample.WindowsPhone.ViewModels;

namespace SterlingExample.WindowsPhone
{
    public partial class MainPage : PhoneApplicationPage
    {
        object _selectedItem;

        public MainPage()
        {
            InitializeComponent();

            SupportedOrientations = SupportedPageOrientation.Portrait;
            Loaded += new RoutedEventHandler(MainPage_Loaded);

            PageTransitionList.Completed += new EventHandler(PageTransitionList_Completed);

            // Set the data context of the listbox control to the sample data
            DataContext = new MainViewModel();
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Reset page transition
            ResetPageTransitionList.Begin();
        }
        private void ListBoxOne_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Capture selected item data
            _selectedItem = (sender as ListBox).SelectedItem;

            // Start page transition animation
            PageTransitionList.Begin();
        }

        private void PageTransitionList_Completed(object sender, EventArgs e)
        {
            // Set datacontext of details page to selected listbox item
            NavigationService.Navigate(new Uri("/DetailsPage.xaml", UriKind.Relative));
            FrameworkElement root = Application.Current.RootVisual as FrameworkElement;
            root.DataContext = _selectedItem;
        }
    }
}