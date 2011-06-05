using System.Windows;
using SterlingRecipes.Contracts;
using SterlingRecipes.Messages;
using SterlingRecipes.Models;
using UltraLight.MVVM;

namespace SterlingRecipes.Views
{
    public partial class IngredientsList
    {
        public IngredientsList()
        {
            InitializeComponent();
            this.BindToViewModel<IRecipeViewModel>(LayoutRoot);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            LayoutRoot.ActivatePage<IRecipeViewModel>();
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            this.DeactivatePage<IRecipeViewModel>();
            base.OnNavigatingFrom(e);
        }

        /// <summary>
        ///     Delete click - go ahead and handle this directly via event aggregator
        /// </summary>
        /// <param name="sender">The button</param>
        /// <param name="e">The arguments</param>
        private void _ButtonClick(object sender, RoutedEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            
            if (frameworkElement == null)
            {
                return;
            }

            var ingredientModel = frameworkElement.DataContext as IngredientModel;

            if (ingredientModel == null)
            {
                return;
            }

            UltraLightLocator.EventAggregator.Publish(IngredientMessage.Create(IngredientAction.Remove, ingredientModel));
        }
    }
}