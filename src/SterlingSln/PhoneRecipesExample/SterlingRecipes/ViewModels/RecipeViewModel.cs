using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SterlingRecipes.Contracts;
using SterlingRecipes.Database;
using SterlingRecipes.Messages;
using SterlingRecipes.Models;
using UltraLight.MVVM;
using UltraLight.MVVM.Contracts;

namespace SterlingRecipes.ViewModels
{
    /// <summary>
    ///     Recipe view model
    /// </summary>
    /// <remarks>
    ///     Handles a text editor message and an ingredient message
    /// </remarks>
    public class RecipeViewModel : BaseViewModel, ITombstoneFriendly, IRecipeViewModel, IEventSink<ITextEditorViewModel>,
                                   IEventSink<IngredientMessage>
    {
        // store the original recipe
        private RecipeModel _recipe;

        /// <summary>
        ///     Use these to flag when events are processed to avoid the tombstone recovery from overwriting
        ///     the message results
        /// </summary>
        private bool _instructionSink;

        private bool _ingredientsSink;

        private const string RECIPE_ID = "RecipeId";
        private const string EDIT_RECIPE = "Edit Recipe";
        private const string ADD_RECIPE = "Add Recipe";

        /// <summary>
        ///     Set it all up and subscribe to events
        /// </summary>
        public RecipeViewModel()
        {
            TextCommand = new ActionCommand<object>(o =>
                                                        {
                                                            UltraLightLocator.GetViewModel<ITextEditorViewModel>().
                                                                BeginEdit("Instructions", Instructions);
                                                            RequestNavigation("/Views/TextEditor.xaml");
                                                        });

            Ingredients = new ObservableCollection<IngredientModel>();

            AddIngredient = new ActionCommand<object>(o => _AddIngredient());

            DeleteIngredient = new ActionCommand<IngredientModel>(_DeleteIngredient, i => i != null);

            SaveRecipe = new ActionCommand<object>(o => _SaveRecipe());

            Cancel = new ActionCommand<object>(o => _Cancel());

            UltraLightLocator.EventAggregator.Subscribe<ITextEditorViewModel>(this);
            UltraLightLocator.EventAggregator.Subscribe<IngredientMessage>(this);
        }

        /// <summary>
        ///     Name of the recipe
        /// </summary>
        private string _recipeName;

        public string RecipeName
        {
            get { return _recipeName; }
            set
            {
                _recipeName = value;
                RaisePropertyChanged(() => RecipeName);
            }
        }

        /// <summary>
        ///     Title (add/edit)
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

        public override bool CancelBackRequest()
        {
            if (
                !Dialog.ShowMessage("Confirm Cancel", "Are you sure you wish to cancel? All changes will be lost!", true))
                return true;

            App.Database.Delete(typeof(TombstoneModel), typeof(IRecipeViewModel));
            return false;
        }

        /// <summary>
        ///     Category for the recipe
        /// </summary>
        private CategoryModel _recipeCategory;

        public CategoryModel RecipeCategory
        {
            get { return _recipeCategory; }
            set
            {
                _recipeCategory = value;
                RaisePropertyChanged(() => RecipeCategory);
            }
        }

        /// <summary>
        ///     Available categories
        /// </summary>
        public IEnumerable<CategoryModel> Categories
        {
            get { return UltraLightLocator.GetViewModel<IMainViewModel>().Categories; }
        }

        /// <summary>
        ///     Ingredients for this recipe
        /// </summary>
        public ObservableCollection<IngredientModel> Ingredients { get; private set; }

        /// <summary>
        ///     Current ingredient to work with
        /// </summary>
        private IngredientModel _selectedIngredient;

        public IngredientModel SelectedIngredient
        {
            get { return _selectedIngredient; }
            set
            {
                _selectedIngredient = value;
                RaisePropertyChanged(() => SelectedIngredient);
                if (value != null)
                {
                    _EditIngredient();
                }
            }
        }

        /// <summary>
        ///     Instructions
        /// </summary>
        private string _instructions;

        public string Instructions
        {
            get { return _instructions; }
            set
            {
                _instructions = value;
                RaisePropertyChanged(() => Instructions);
            }
        }

        /// <summary>
        ///     Edit the text (instructions)
        /// </summary>
        public IActionCommand<object> TextCommand { get; private set; }

        /// <summary>
        ///     Add a new ingredient
        /// </summary>
        public IActionCommand<object> AddIngredient { get; private set; }

        /// <summary>
        ///     Delete the existing ingredient
        /// </summary>
        public IActionCommand<IngredientModel> DeleteIngredient { get; private set; }

        /// <summary>
        ///     Save the recipe
        /// </summary>
        public IActionCommand<object> SaveRecipe { get; private set; }

        /// <summary>
        ///     Cancel
        /// </summary>
        public IActionCommand<object> Cancel { get; private set; }

        /// <summary>
        ///     Bindings
        /// </summary>
        public override IEnumerable<IActionCommand<object>> ApplicationBindings
        {
            get { return new[] {AddIngredient, SaveRecipe, Cancel}; }
        }

        /// <summary>
        ///     Delete the ingredient
        /// </summary>
        /// <param name="ingredientModel">The ingredient to delete</param>
        private void _DeleteIngredient(IngredientModel ingredientModel)
        {
            Ingredients.Remove(ingredientModel);
            RaisePropertyChanged(() => Ingredients);
        }

        /// <summary>
        ///     Edit an ingredient
        /// </summary>
        private void _EditIngredient()
        {
            UltraLightLocator.GetViewModel<IIngredientViewModel>().EditIngredient(SelectedIngredient);
            SelectedIngredient = null;
            RequestNavigation("/Views/IngredientEdit.xaml");
        }

        /// <summary>
        ///     Add a new ingredient
        /// </summary>
        private void _AddIngredient()
        {
            UltraLightLocator.GetViewModel<IIngredientViewModel>().AddIngredient();
            SelectedIngredient = null;
            RequestNavigation("/Views/IngredientEdit.xaml");
        }

        /// <summary>
        ///     Save the recipe
        /// </summary>
        private void _SaveRecipe()
        {
            const string CANNOT_SAVE = "Cannot Save";

            if (string.IsNullOrEmpty(RecipeName))
            {
                Dialog.ShowMessage(CANNOT_SAVE, "The recipe name is required.", false);
                return;
            }

            if (string.IsNullOrEmpty(Instructions))
            {
                Dialog.ShowMessage(CANNOT_SAVE, "You must enter instructions.", false);
                return;
            }

            if (Ingredients.Count < 1)
            {
                Dialog.ShowMessage(CANNOT_SAVE, "You need at least one ingredient.", false);
                return;
            }

            _recipe.Name = RecipeName;
            _recipe.Instructions = Instructions;
            _recipe.Category = RecipeCategory;

            // set up ingredients on the new recipe
            if (_recipe.Ingredients != null)
            {
                _recipe.Ingredients.Clear();
            }
            else
            {
                _recipe.Ingredients = new List<IngredientModel>();
            }

            // purge any recipes that were removed
            if (_recipe.Id > 0)
            {
                var recipeOnDisk = App.Database.Load<RecipeModel>(_recipe.Id);
                foreach (var ingredient in from ingredient in recipeOnDisk.Ingredients
                                           let exists =
                                               (from i in Ingredients where i.Id == ingredient.Id select i).Any()
                                           where !exists
                                           select ingredient)
                {
                    App.Database.Delete(typeof (IngredientModel), ingredient.Id);
                }
            }
            else
            {
                // generate an id
                App.Database.Save(_recipe);
            }

            var order = 0;

            // iterate the ingredients
            foreach (var ingredient in Ingredients.OrderBy(i => i.Order))
            {
                ingredient.Recipe = _recipe;
                ingredient.Order = ++order;

                // generate an id for the ingredient
                if (ingredient.Id < 1)
                {
                    App.Database.Save(ingredient);
                }

                _recipe.Ingredients.Add(ingredient);
            }

            App.Database.Save(_recipe);

            App.Database.Delete(typeof (TombstoneModel), typeof (IRecipeViewModel));
            App.Database.Flush(); // significant changes so let's make sure we get these on disk

            GoBack();
        }

        /// <summary>
        ///     Cancel
        /// </summary>
        private void _Cancel()
        {
            if (
                !Dialog.ShowMessage("Confirm Cancel", "Are you sure you wish to cancel? All changes will be lost!", true))
                return;

            App.Database.Delete(typeof (TombstoneModel), typeof (IRecipeViewModel));
            GoBack();
        }

        /// <summary>
        ///     Edit the recipe
        /// </summary>
        /// <param name="recipe"></param>
        public void EditRecipe(RecipeModel recipe)
        {
            Title = EDIT_RECIPE;
            _recipe = App.Database.Load<RecipeModel>(recipe.Id);
            RecipeName = _recipe.Name;
            Instructions = _recipe.Instructions;
            Ingredients.Clear();
            foreach (var ingredient in _recipe.Ingredients)
            {
                Ingredients.Add(ingredient);
            }
            _selectedIngredient = null;
            RaisePropertyChanged(() => SelectedIngredient);
            RaisePropertyChanged(() => Categories);
            RecipeCategory = recipe.Category;

            App.Database.Delete(typeof (TombstoneModel), typeof (IRecipeViewModel));
            _instructionSink = false;
            _ingredientsSink = false;
        }

        /// <summary>
        ///     Add a new recipe
        /// </summary>
        public void AddRecipe()
        {
            _recipe = new RecipeModel();
            Title = ADD_RECIPE;
            RecipeName = string.Empty;
            Instructions = string.Empty;
            Ingredients.Clear();
            SelectedIngredient = null;
            RecipeCategory = UltraLightLocator.GetViewModel<IMainViewModel>().CurrentCategory ??
                             UltraLightLocator.GetViewModel<IMainViewModel>().Categories.First();
            RaisePropertyChanged(() => Categories);
            App.Database.Delete(typeof (TombstoneModel), typeof (IRecipeViewModel));
            _instructionSink = false;
            _ingredientsSink = false;
        }

        /// <summary>
        ///     Handle the text edit event (update the instructions)
        /// </summary>
        /// <param name="publishedEvent">The view model handling the text</param>
        public void HandleEvent(ITextEditorViewModel publishedEvent)
        {
            if (publishedEvent == null) return;

            if (string.IsNullOrEmpty(publishedEvent.Text)) return;

            Instructions = publishedEvent.Text;
            _instructionSink = true;
        }

        /// <summary>
        ///     Tombstone
        /// </summary>
        public void Deactivate()
        {
            if (_recipe == null)
            {
                return;
            }
            var tombstone = new TombstoneModel {SyncType = typeof (IRecipeViewModel)};
            tombstone.State.Add(RECIPE_ID, _recipe.Id);
            tombstone.State.Add(ExtractPropertyName(() => RecipeCategory), RecipeCategory.Id);
            tombstone.State.Add(ExtractPropertyName(() => RecipeName), RecipeName);
            tombstone.State.Add(ExtractPropertyName(() => Title), Title);
            tombstone.State.Add(ExtractPropertyName(() => Instructions), Instructions);

            var ingredients = Ingredients.Where(i => i != null).OrderBy(i => i.Order)
                .Select(ingredient => new IngredientCacheModel
                                          {
                                              IngredientId = ingredient.Id,
                                              Food = ingredient.Food,
                                              Amount = ingredient.Amount
                                          }).ToList();
            tombstone.State.Add(ExtractPropertyName(() => Ingredients), ingredients);
            App.Database.Save(tombstone);
        }

        /// <summary>
        ///     Return from tombstone
        /// </summary>
        public void Activate()
        {
            var tombstone = App.Database.Load<TombstoneModel>(typeof (IRecipeViewModel));
            if (tombstone == null) return;
            var recipeId = tombstone.TryGet(RECIPE_ID, -1);
            _recipe = recipeId > 0 ? App.Database.Load<RecipeModel>(recipeId) : new RecipeModel();

            var categoryId = tombstone.TryGet(ExtractPropertyName(() => RecipeCategory), -1);
            if (categoryId > 0)
            {
                RecipeCategory = (from c in UltraLightLocator.GetViewModel<IMainViewModel>().Categories
                                  where c.Id == categoryId
                                  select c).FirstOrDefault();
            }

            RecipeName = tombstone.TryGet(ExtractPropertyName(() => RecipeName), string.Empty);
            Title = tombstone.TryGet(ExtractPropertyName(() => Title), string.Empty);

            if (_instructionSink)
            {
                // again, event aggregator sent message so don't overwrite with tombstone
                _instructionSink = false;
            }
            else
            {
                Instructions = tombstone.TryGet(ExtractPropertyName(() => Instructions), string.Empty);
            }

            if (_ingredientsSink)
            {
                _ingredientsSink = false;
            }
            else
            {
                // rebuild ingredients
                Ingredients.Clear();
                var order = 1;
                foreach (var ingredientModel in
                    from ingredient in tombstone.TryGet(ExtractPropertyName(() => Ingredients),
                                                        Enumerable.Empty<IngredientCacheModel>())
                    select new IngredientModel
                               {
                                   Id = ingredient.IngredientId,
                                   Order = order++,
                                   Food = ingredient.Food,
                                   Amount = ingredient.Amount
                               })
                {
                    Ingredients.Add(ingredientModel);
                }
            }
        }

        /// <summary>
        ///     Handle an ingredient event (update/remove)
        /// </summary>
        /// <param name="publishedEvent"></param>
        public void HandleEvent(IngredientMessage publishedEvent)
        {
            if (publishedEvent.Action.Equals(IngredientAction.Save))
            {
                SelectedIngredient = null;
                var existingIngredient =
                    (from i in Ingredients where i.Id == publishedEvent.Model.Id select i).FirstOrDefault();
                if (existingIngredient != null)
                {
                    Ingredients.Remove(existingIngredient);
                }
                Ingredients.Add(publishedEvent.Model);
                _ingredientsSink = true;
            }
            else
            {
                if (DeleteIngredient.CanExecute(publishedEvent.Model))
                {
                    DeleteIngredient.Execute(publishedEvent.Model);
                }
            }
        }
    }
}