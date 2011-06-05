using System;
using System.Collections.Generic;
using SterlingRecipes.Contracts;
using UltraLight.MVVM.Contracts;

namespace SterlingRecipes.SampleData
{
    /// <summary>
    ///     Design time text data
    /// </summary>
    public class DesignTextEditorViewModel : ITextEditorViewModel
    {
        public void BeginEdit(string title, string text)
        {
            return;
        }

        public string Title
        {
            get { return "Instructions"; }
        }

        public string Text
        {
            get
            {
                return
                    "Remove crust and flatten bread with rolling pin by rolling bread on cutting board. Spread a thin layer of almond butter and jam onto bread. Roll up slice of bread to look like a sushi roll. Cut bread into four bite-size pieces using a serrated knife.";
            }
        }

        /// <summary>
        ///     To to visual state 
        /// </summary>
        public Action<string, bool> GoToVisualState { get; set; }

        /// <summary>
        ///     Back request
        /// </summary>
        /// <returns>Used to process a back request. Return true to cancel.</returns>
        public bool CancelBackRequest()
        {
            return false;
        }

        /// <summary>
        ///     Navigation 
        /// </summary>
        public Action GoBack { get; set; }

        /// <summary>
        ///     Dialog 
        /// </summary>
        public IDialog Dialog { get; set; }

        /// <summary>
        ///     Navigation request
        /// </summary>
        public Action<string> RequestNavigation { get; set; }

        /// <summary>
        ///     Commands to bind to application bar buttons
        /// </summary>
        public IEnumerable<IActionCommand<object>> ApplicationBindings { get; set; }
    }
}