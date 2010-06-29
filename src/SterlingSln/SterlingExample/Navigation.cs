using System;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SterlingExample.Model;
using SterlingExample.Views;

namespace SterlingExample
{
    /// <summary>
    ///     Simple navigation class
    /// </summary>
    public class Navigation
    {
        /// <summary>
        ///     Set the view
        /// </summary>
        private readonly Action<UserControl> _setView;

        private readonly Lazy<UserControl> _requestStorage = new Lazy<UserControl>(()=>new RequestStorage());
        private readonly Lazy<UserControl> _mainView = new Lazy<UserControl>(()=>new MainView());
        private readonly Lazy<UserControl> _buildView = new Lazy<UserControl>(()=>new BuildView());

        /// <summary>
        ///     Navigation
        /// </summary>
        /// <param name="setView">Set the view</param>
        public Navigation(Action<UserControl> setView)
        {
            _setView = setView;
            _BeginNavigation();
        }

        /// <summary>
        ///     Begin the navigation
        /// </summary>
        private void _BeginNavigation()
        {
            // do we have enough storage?
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (iso.Quota < SterlingService.QUOTA)
                {
                    NavigateToRequestView();
                }
                else
                {
                    if (SterlingService.Current.Database.Query<FoodGroup,int>().Count() > 0)
                    {
                        NavigateToMainView();
                    }
                    else
                    {
                        NavigateToBuildView();
                    }
                }
            }
        }

        public void NavigateToMainView()
        {
            _setView(_mainView.Value);
        }

        public void NavigateToBuildView()
        {
            _setView(_buildView.Value);
            SterlingService.Current.RequestRebuild();
        }

        public void NavigateToRequestView()
        {
            _setView(_requestStorage.Value);            
        }
    }
}
