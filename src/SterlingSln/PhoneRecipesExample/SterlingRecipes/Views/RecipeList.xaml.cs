using SterlingRecipes.Contracts;
using UltraLight.MVVM;

namespace SterlingRecipes.Views
{
    public partial class RecipeList
    {
        public RecipeList()
        {
            InitializeComponent();
            this.BindToViewModel<IMainViewModel>(LayoutRoot);
        }        
    }
}
