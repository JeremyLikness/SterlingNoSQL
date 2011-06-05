using UltraLight.MVVM.Contracts;

namespace SterlingRecipes.Contracts
{
    /// <summary>
    ///     Contract for the text 
    /// </summary>
    public interface ITextEditorViewModel : IViewModel
    {
        void BeginEdit(string title, string text);

        string Title { get; }

        string Text { get; }
    }
}