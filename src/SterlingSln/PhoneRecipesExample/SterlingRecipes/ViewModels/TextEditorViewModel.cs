using System.Collections.Generic;
using SterlingRecipes.Contracts;
using SterlingRecipes.Models;
using UltraLight.MVVM;
using UltraLight.MVVM.Contracts;

namespace SterlingRecipes.ViewModels
{
    /// <summary>
    ///     Allows a generic text edit dialog
    /// </summary>
    public class TextEditorViewModel : BaseViewModel, ITextEditorViewModel, ITombstoneFriendly
    {
        /// <summary>
        ///     Set it up
        /// </summary>
        public TextEditorViewModel()
        {
            OKCommand = new ActionCommand<object>(o =>
                                                      {
                                                          App.Database.Delete(typeof (TombstoneModel),
                                                                              typeof (ITextEditorViewModel));
                                                          UltraLightLocator.EventAggregator.Publish
                                                              <ITextEditorViewModel>(this);
                                                          GoBack();
                                                      },
                                                  o => !string.IsNullOrEmpty(Text));

            CancelCommand = new ActionCommand<object>(o => _CheckCancel());
        }

        /// <summary>
        ///     For pages, the list of commands to bind to application bar buttons
        /// </summary>
        public override IEnumerable<IActionCommand<object>> ApplicationBindings
        {
            get { return new[] {OKCommand, CancelCommand}; }
        }

        /// <summary>
        ///     Title (pass whatever to prompt what is being edited)
        /// </summary>
        private string _title;

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                RaisePropertyChanged(() => Title);
            }
        }

        /// <summary>
        ///     Text to edit
        /// </summary>
        private string _text;

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                RaisePropertyChanged(() => Text);
                OKCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        ///  OK - save it
        /// </summary>
        public IActionCommand<object> OKCommand { get; private set; }

        /// <summary>
        ///     Nope, cancel
        /// </summary>
        public IActionCommand<object> CancelCommand { get; private set; }

        public override bool CancelBackRequest()
        {
            if (
                !Dialog.ShowMessage("Confirm Cancel", "Are you sure you wish to cancel? All changes will be lost!", true))
                return true;
            App.Database.Delete(typeof(TombstoneModel), typeof(ITextEditorViewModel));
            return false;
        }

        /// <summary>
        ///     Make sure they want to cancel
        /// </summary>
        private void _CheckCancel()
        {
            if (
                !Dialog.ShowMessage("Confirm Cancel", "Are you sure you wish to cancel? All changes will be lost!", true))
                return;
            App.Database.Delete(typeof (TombstoneModel), typeof (ITextEditorViewModel));
            GoBack();
        }

        /// <summary>
        ///     Begin the edit stage
        /// </summary>
        /// <param name="title">Title to show</param>
        /// <param name="text">Existing text</param>
        public void BeginEdit(string title, string text)
        {
            App.Database.Delete(typeof (TombstoneModel), typeof (ITextEditorViewModel));
            Title = title;
            Text = text;
        }

        /// <summary>
        ///     Tombstone
        /// </summary>
        public void Deactivate()
        {
            var tombstone = new TombstoneModel {SyncType = typeof (ITextEditorViewModel)};
            tombstone.State.Add(ExtractPropertyName(() => Title), Title);
            tombstone.State.Add(ExtractPropertyName(() => Text), Text);
            App.Database.Save(tombstone);
        }

        /// <summary>
        ///     Returned from tombstone
        /// </summary>
        public void Activate()
        {
            var tombstone = App.Database.Load<TombstoneModel>(typeof (ITextEditorViewModel));
            if (tombstone == null) return;
            Title = tombstone.TryGet(ExtractPropertyName(() => Title), string.Empty);
            Text = tombstone.TryGet(ExtractPropertyName(() => Text), string.Empty);
        }
    }
}