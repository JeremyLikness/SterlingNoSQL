using System.Windows.Controls;
using SterlingRecipes.Contracts;
using UltraLight.MVVM;

namespace SterlingRecipes.Views
{
    public partial class FoodSearch
    {
        /// <summary>
        ///     Set it up
        /// </summary>
        public FoodSearch()
        {
            InitializeComponent();
            this.BindToViewModel<IIngredientViewModel>(LayoutRoot);
        }

        /// <summary>
        ///     This allows the updates to be sent as the user is typing to auto-update the
        ///     list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _TextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == null) return;
            var binding = tb.GetBindingExpression(TextBox.TextProperty);
            if (binding == null) return;
            binding.UpdateSource();
        }
    }
}