using System.ComponentModel;
using SterlingExample.Database;

namespace SterlingExample.Views
{
    /// <summary>
    ///     View to spit out debug information from Sterling
    /// </summary>
    public partial class DebugView
    {
        /// <summary>
        ///     Initialize it and set up the logger
        /// </summary>
        public DebugView()
        {
            InitializeComponent();
            Loaded += (o, e) => LayoutRoot.DataContext = DesignerProperties.IsInDesignTool
                                                             ? new UILogger()
                                                             : SterlingService.Current.Logger; 
        }
    }
}
