using System.Windows.Controls;
using SterlingRecipes.Contracts;
using UltraLight.MVVM;

namespace SterlingRecipes.Views
{
    public partial class TextEditor
    {
        public TextEditor()
        {
            InitializeComponent();
            this.BindToViewModel<ITextEditorViewModel>(LayoutRoot);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            LayoutRoot.ActivatePage<ITextEditorViewModel>();
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            this.DeactivatePage<ITextEditorViewModel>();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((TextBox)sender).GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }
    }
}