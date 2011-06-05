using SterlingRecipes.Contracts;
using UltraLight.MVVM;

namespace SterlingRecipes.Views
{
    public partial class IngredientEdit
    {
        public IngredientEdit()
        {
            InitializeComponent();
            this.BindToViewModel<IIngredientViewModel>(LayoutRoot);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            LayoutRoot.ActivatePage<IIngredientViewModel>();
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            this.DeactivatePage<IIngredientViewModel>();
            base.OnNavigatedFrom(e);
        }
    }
}