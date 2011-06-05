using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SterlingRecipes.Contracts;
using SterlingRecipes.Database;
using SterlingRecipes.Models;
using UltraLight.MVVM;
using UltraLight.MVVM.Contracts;

namespace SterlingRecipes.ViewModels
{
    /// <summary>
    ///     The main view model
    /// </summary>
    public class MainViewModel : BaseViewModel, IMainViewModel, ITombstoneFriendly
    {
        /// <summary>
        ///     Set it all up and grab the list of categories 
        /// </summary>
        public MainViewModel()
        {
            Categories = new ObservableCollection<CategoryModel>();

            foreach (var category in App.Database.Query<CategoryModel, int>())
            {
                Categories.Add(category.LazyValue.Value);
            }

            EditRecipe = new ActionCommand<object>(o => _EditRecipe(), o => _CanEditRecipe());

            AddRecipe = new ActionCommand<object>(o => _AddRecipe());

            DeleteRecipe = new ActionCommand<object>(o => _DeleteRecipe(), o => _CanEditRecipe());
        }

        /// <summary>
        ///     Recipe list is 
        /// </summary>
        public IEnumerable<RecipeModel> Recipes
        {
            get
            {
                return
                    from r in
                        App.Database.Query<RecipeModel, int, string, int>(RecipeDatabase.IDX_RECIPE_CATEGORYID_NAME)
                    where r.Index.Item1.Equals(CurrentCategory == null ? 0 : CurrentCategory.Id)
                    orderby r.Index.Item2
                    select new RecipeModel {Id = r.Key, Name = r.Index.Item2};
            }
        }

        /// <summary>
        ///     Edit the recipe
        /// </summary>
        public IActionCommand<object> EditRecipe { get; private set; }

        /// <summary>
        ///     Add a new recipe
        /// </summary>
        public IActionCommand<object> AddRecipe { get; private set; }

        /// <summary>
        ///     Delete the recipe
        /// </summary>
        public IActionCommand<object> DeleteRecipe { get; private set; }

        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        public ObservableCollection<CategoryModel> Categories { get; private set; }

        /// <summary>
        ///     Current recipe being worked on
        /// </summary>
        private RecipeModel _currentRecipe;

        public RecipeModel CurrentRecipe
        {
            get { return _currentRecipe; }
            set
            {
                _currentRecipe = value;
                RaisePropertyChanged(() => CurrentRecipe);
                EditRecipe.RaiseCanExecuteChanged();
                DeleteRecipe.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        ///     Current category selected
        /// </summary>
        private CategoryModel _currentCategory;

        public CategoryModel CurrentCategory
        {
            get { return _currentCategory; }
            set
            {
                _currentCategory = value;
                RaisePropertyChanged(() => CurrentCategory);
                RaisePropertyChanged(() => Recipes);
            }
        }

        /// <summary>
        ///     Add a new recipe
        /// </summary>
        private void _AddRecipe()
        {
            UltraLightLocator.GetViewModel<IRecipeViewModel>().AddRecipe();
            RequestNavigation("/Views/RecipeEdit.xaml");
        }

        /// <summary>
        ///     Edit recipe
        /// </summary>
        private void _EditRecipe()
        {
            UltraLightLocator.GetViewModel<IRecipeViewModel>().EditRecipe(
                App.Database.Load<RecipeModel>(CurrentRecipe.Id));
            RequestNavigation("/Views/RecipeEdit.xaml");
        }

        /// <summary>
        ///     Delete an existing recipe with confirmation
        /// </summary>
        private void _DeleteRecipe()
        {
            if (!Dialog.ShowMessage("Confirm Delete", "Are you sure you wish to delete the recipe?", true)) return;

            var currentRecipe = App.Database.Load<RecipeModel>(CurrentRecipe.Id);

            foreach (var ingredient in currentRecipe.Ingredients)
            {
                App.Database.Delete(ingredient);
            }
            App.Database.Delete(currentRecipe);
            CurrentRecipe = null;

            // refresh the recipe list
            RaisePropertyChanged(() => Recipes);
        }

        /// <summary>
        ///     Can we edit?
        /// </summary>
        /// <returns>True if a recipe is selected</returns>
        private bool _CanEditRecipe()
        {
            return CurrentRecipe != null;
        }

        /// <summary>
        ///     Application buttons
        /// </summary>
        public override IEnumerable<IActionCommand<object>> ApplicationBindings
        {
            get { return new[] {AddRecipe, EditRecipe, DeleteRecipe}; }
        }

        /// <summary>
        ///     Tombstone
        /// </summary>
        public void Deactivate()
        {
            var tombstone = new TombstoneModel {SyncType = typeof (IMainViewModel)};
            if (CurrentCategory != null)
            {
                tombstone.State.Add(ExtractPropertyName(() => CurrentCategory), CurrentCategory.Id);
            }
            if (CurrentRecipe != null)
            {
                tombstone.State.Add(ExtractPropertyName(() => CurrentRecipe), CurrentRecipe.Id);
            }
            App.Database.Save(tombstone);
        }

        /// <summary>
        ///     Return from tombstone
        /// </summary>
        public void Activate()
        {
            var saved = App.Database.Load<TombstoneModel>(typeof (IMainViewModel));

            if (saved == null) return;

            var categoryId = saved.TryGet(ExtractPropertyName(() => CurrentCategory), 0);

            if (categoryId > 0)
            {
                CurrentCategory = (from c in Categories where c.Id == categoryId select c).FirstOrDefault();
            }
            var currentRecipeId = saved.TryGet(ExtractPropertyName(() => CurrentRecipe), 0);
            if (currentRecipeId > 0)
            {
                CurrentRecipe = (from r in Recipes where r.Id == currentRecipeId select r).FirstOrDefault();
            }
        }
    }
}