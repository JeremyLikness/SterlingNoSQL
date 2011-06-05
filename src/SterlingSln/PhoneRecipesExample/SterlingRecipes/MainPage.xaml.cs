using SterlingRecipes.Contracts;
using UltraLight.MVVM;

namespace SterlingRecipes
{
    public partial class MainPage
    {        
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            this.BindToViewModel<IMainViewModel>(LayoutRoot);            
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            pivotMain.ActivatePage<IMainViewModel>();
            base.OnNavigatedTo(e);            
        }
        
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            this.DeactivatePage<IMainViewModel>();
            base.OnNavigatedFrom(e);
        }
    }
}