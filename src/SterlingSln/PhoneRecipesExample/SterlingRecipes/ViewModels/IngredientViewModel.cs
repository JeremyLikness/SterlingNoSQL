using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SterlingRecipes.Contracts;
using SterlingRecipes.Database;
using SterlingRecipes.Messages;
using SterlingRecipes.Models;
using UltraLight.MVVM;
using UltraLight.MVVM.Contracts;

namespace SterlingRecipes.ViewModels
{
    /// <summary>
    ///     Ingredient view model - handles editing/deleting ingredients
    /// </summary>
    public class IngredientViewModel : BaseViewModel, IIngredientViewModel, ITombstoneFriendly
    {
        private const string TAP_TO_SELECT = "(Tap to Select)";
        private const string INGREDIENT_ID = "IngredientId";

        private int _ingredientId;
        private static int _newId = -100;
        private bool _foodSearch;

        /// <summary>
        ///     Food search
        /// </summary>
        private string _foodText;

        public string FoodText
        {
            get { return _foodText; }
            set
            {
                _foodText = value;
                RaisePropertyChanged(() => FoodText);
                RaisePropertyChanged(() => Food);
                AddFood.RaiseCanExecuteChanged();
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

        /// <summary>
        ///     Units
        /// </summary>
        private string _units;

        public string Units
        {
            get { return _units; }
            set
            {
                _units = value;
                RaisePropertyChanged(() => Units);
            }
        }

        /// <summary>
        ///     Food filter based on entered text
        /// </summary>
        public IEnumerable<FoodModel> Food
        {
            get
            {
                if (string.IsNullOrEmpty(_foodText))
                {
                    return Enumerable.Empty<FoodModel>();
                }
                var foodTextLower = _foodText.ToLower();
                return from f in App.Database.Query<FoodModel, string, int>(RecipeDatabase.IDX_FOOD_NAME)
                       where f.Index.ToLower().Contains(foodTextLower)
                       orderby f.Index
                       select new FoodModel {Id = f.Key, FoodName = f.Index};
            }
        }

        /// <summary>
        ///     Food item selected
        /// </summary>
        private FoodModel _selectedFood;

        public FoodModel SelectedFood
        {
            get { return _selectedFood; }
            set
            {
                _selectedFood = value;
                RaisePropertyChanged(() => SelectedFood);

                if (value == null) return;

                // if not null, set the text to the selected item

                FoodText = value.FoodName;

                if (!_foodSearch) return;

                // if in the food search, we've selected something so go back

                GoBack();
            }
        }

        /// <summary>
        ///     List of measurements - using the query ensures they are cached after first load
        /// </summary>
        public IEnumerable<MeasureModel> Measures
        {
            get
            {
                return from m in App.Database.Query<MeasureModel, int>()
                       orderby m.LazyValue.Value.FullMeasure
                       select m.LazyValue.Value;
            }
        }

        /// <summary>
        ///     Selected measure
        /// </summary>
        private MeasureModel _selectedMeasure;

        public MeasureModel SelectedMeasure
        {
            get { return _selectedMeasure; }
            set
            {
                _selectedMeasure = value;
                RaisePropertyChanged(() => SelectedMeasure);
            }
        }

        /// <summary>
        ///  Save an ingredient (update/add)
        /// </summary>
        public IActionCommand<object> SaveIngredient { get; private set; }

        /// <summary>
        ///     Cancel transaction
        /// </summary>
        public IActionCommand<object> Cancel { get; private set; }

        /// <summary>
        ///     Add a new food
        /// </summary>
        public IActionCommand<object> AddFood { get; private set; }

        /// <summary>
        ///     Start a food search
        /// </summary>
        public IActionCommand<object> FoodSearch { get; private set; }

        /// <summary>
        ///     Option buttons
        /// </summary>
        public override IEnumerable<IActionCommand<object>> ApplicationBindings
        {
            get { return new[] {SaveIngredient, Cancel}; }
        }

        /// <summary>
        ///     Set it all up
        /// </summary>
        public IngredientViewModel()
        {
            SaveIngredient = new ActionCommand<object>(o => _SaveIngredient());
            AddFood = new ActionCommand<object>(o => _AddFood(), o => !string.IsNullOrEmpty(FoodText));
            Cancel = new ActionCommand<object>(o => _Cancel());
            FoodSearch = new ActionCommand<object>(o => _FoodSearch());
        }

        /// <summary>
        ///     Requested a food search, set the flag (we use the same view model) and then navigate
        /// </summary>
        private void _FoodSearch()
        {
            _foodSearch = true;
            _foodText = string.Empty;
            RaisePropertyChanged(() => FoodText);
            RequestNavigation("/Views/FoodSearch.xaml");
        }

        /// <summary>
        ///     Add a new food - make sure it doesn't exist and then save it, trigger will
        ///     auto-generate the id
        /// </summary>
        private void _AddFood()
        {
            var exists = (from f in App.Database.Query<FoodModel, string, int>(RecipeDatabase.IDX_FOOD_NAME)
                          where f.Index.ToLower().Equals(FoodText.ToLower())
                          select f).FirstOrDefault();

            if (exists != null)
            {
                SelectedFood = exists.LazyValue.Value;
                return;
            }

            var food = new FoodModel {FoodName = FoodText};
            App.Database.Save(food);
            App.Database.Flush();
            SelectedFood = food;
        }

        public override bool CancelBackRequest()
        {
            _Cancel();
            return false;
        }

        /// <summary>
        ///     Cancel - go back
        /// </summary>
        private void _Cancel()
        {
            App.Database.Delete(typeof (TombstoneModel), typeof (IIngredientViewModel));
            _ingredientId = -1;
            GoBack();
        }

        /// <summary>
        ///     Save the new ingredient
        /// </summary>
        private void _SaveIngredient()
        {
            const string CANNOT_SAVE = "Cannot Save";

            double units;

            if (!double.TryParse(Units, out units))
            {
                Dialog.ShowMessage(CANNOT_SAVE, "Units must be a valid number.", false);
                return;
            }

            if (units <= 0)
            {
                Dialog.ShowMessage(CANNOT_SAVE, "Units must be greater than 0.", false);
                return;
            }

            if (SelectedFood == null)
            {
                Dialog.ShowMessage(CANNOT_SAVE, "You must select a food.", false);
                return;
            }

            // set up a negative id to keep track of new ingredients
            if (_ingredientId < 1)
            {
                _ingredientId = _newId;
                Interlocked.Decrement(ref _newId);
            }

            // set this up to save it
            var ingredient = new IngredientModel
                                 {
                                     Id = _ingredientId,
                                     Amount =
                                         new MeasureAmountModel
                                             {
                                                 Measure = App.Database.Load<MeasureModel>(SelectedMeasure.Id),
                                                 Units = units
                                             },
                                     Food = App.Database.Load<FoodModel>(SelectedFood.Id)
                                 };

            UltraLightLocator.EventAggregator.Publish(IngredientMessage.Create(IngredientAction.Save, ingredient));
            App.Database.Delete(typeof (TombstoneModel), typeof (IIngredientViewModel));
            GoBack();
        }

        /// <summary>
        ///  Begin an edit transaction
        /// </summary>
        /// <param name="ingredientModel">The ingredient to edit</param>
        public void EditIngredient(IngredientModel ingredientModel)
        {
            App.Database.Delete(typeof (TombstoneModel), typeof (IIngredientViewModel));
            _ingredientId = ingredientModel.Id;
            Title = "Edit Ingredient";
            FoodText = ingredientModel.Food.FoodName;
            SelectedFood = App.Database.Load<FoodModel>(ingredientModel.Food.Id);
            Units = ingredientModel.Amount.Units.ToString();
            SelectedMeasure = App.Database.Load<MeasureModel>(ingredientModel.Amount.Measure.Id);
        }

        /// <summary>
        ///  Add a new ingredient
        /// </summary>
        public void AddIngredient()
        {
            App.Database.Delete(typeof (TombstoneModel), typeof (IIngredientViewModel));
            _ingredientId = -1;
            Title = "Add Ingredient";
            FoodText = TAP_TO_SELECT;
            SelectedFood = null;
            Units = "0";
            SelectedMeasure = Measures.First();
        }


        /// <summary>
        ///     Tombstone
        /// </summary>
        public void Deactivate()
        {
            var tombstone = new TombstoneModel {SyncType = typeof (IIngredientViewModel)};
            tombstone.State.Add(ExtractPropertyName(() => Title), Title);
            tombstone.State.Add(ExtractPropertyName(() => FoodText), FoodText);
            tombstone.State.Add(ExtractPropertyName(() => Units), Units);
            tombstone.State.Add(ExtractPropertyName(() => SelectedMeasure),
                                SelectedMeasure == null ? -1 : SelectedMeasure.Id);
            tombstone.State.Add(ExtractPropertyName(() => SelectedFood), SelectedFood == null ? -1 : SelectedFood.Id);
            tombstone.State.Add(INGREDIENT_ID, _ingredientId);
            App.Database.Save(tombstone);
        }

        /// <summary>
        ///     Returned from tombstone
        /// </summary>
        public void Activate()
        {
            var tombstone = App.Database.Load<TombstoneModel>(typeof (IIngredientViewModel));
            if (tombstone == null) return;
            Title = tombstone.TryGet(ExtractPropertyName(() => Title), string.Empty);
            Units = tombstone.TryGet(ExtractPropertyName(() => Units), "0");
            _ingredientId = tombstone.TryGet(INGREDIENT_ID, -1);
            var selectedMeasureId = tombstone.TryGet(ExtractPropertyName(() => SelectedMeasure), -1);
            if (selectedMeasureId > 0)
            {
                SelectedMeasure = (from m in Measures where m.Id == selectedMeasureId select m).FirstOrDefault();
            }

            if (_foodSearch)
            {
                _foodSearch = false;
                if (SelectedFood != null)
                {
                    FoodText = SelectedFood.FoodName;
                }
            }
            else
            {
                FoodText = tombstone.TryGet(ExtractPropertyName(() => FoodText), TAP_TO_SELECT);
                var selectedFoodId = tombstone.TryGet(ExtractPropertyName(() => SelectedFood), -1);
                if (selectedFoodId > 0)
                {
                    SelectedFood = App.Database.Load<FoodModel>(selectedFoodId);
                }
            }
            if (string.IsNullOrEmpty(_foodText))
            {
                FoodText = TAP_TO_SELECT;
            }
        }
    }
}