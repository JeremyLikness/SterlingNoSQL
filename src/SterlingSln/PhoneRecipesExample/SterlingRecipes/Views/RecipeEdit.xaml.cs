using SterlingRecipes.Contracts;
using UltraLight.MVVM;

namespace SterlingRecipes.Views
{
    public partial class RecipeEdit
    {
        public RecipeEdit()
        {
            InitializeComponent();
            this.BindToViewModel<IRecipeViewModel>(LayoutRoot);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            LayoutRoot.ActivatePage<IRecipeViewModel>();
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            this.DeactivatePage<IRecipeViewModel>();
        }
    }
}