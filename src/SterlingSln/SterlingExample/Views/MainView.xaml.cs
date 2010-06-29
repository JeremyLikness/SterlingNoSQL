using System.Windows.Controls;

namespace SterlingExample.Views
{
    public partial class MainView
    {
        /// <summary>
        ///     The main view
        /// </summary>
        public MainView()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     When it has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var binding = ((TextBox) sender).GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();
        }
    }
}
