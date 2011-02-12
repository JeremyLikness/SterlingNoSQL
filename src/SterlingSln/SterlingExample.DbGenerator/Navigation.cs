using System;
using System.IO.IsolatedStorage;
using System.Windows.Controls;
using SterlingExample.DbGenerator.Views;

namespace SterlingExample.DbGenerator
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
        private readonly Lazy<UserControl> _buildView = new Lazy<UserControl>(()=>new BuildView());
        private readonly Lazy<UserControl> _downloadView = new Lazy<UserControl>(()=>new DownloadView());

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
                    NavigateToBuildView();                    
                }
            }
        }

        public void NavigateToBuildView()
        {
            _setView(_buildView.Value);
            SterlingService.Current.RequestRebuild();
        }

        public void NavigateToDownloadView()
        {
            _setView(_downloadView.Value);
        }

        public void NavigateToRequestView()
        {
            _setView(_requestStorage.Value);            
        }
    }
}
