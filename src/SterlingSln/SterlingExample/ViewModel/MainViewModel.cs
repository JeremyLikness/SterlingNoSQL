using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using SterlingExample.Database;
using SterlingExample.Model;

namespace SterlingExample.ViewModel
{
    /// <summary>
    ///     Main view model
    /// </summary>
    public class MainViewModel : BaseNotify
    {
        public MainViewModel()
        {
            // Command to clear the group selection
            ClearGroup = new DelegateCommand<object>(obj=>CurrentGroup=null,obj=>CurrentGroup!=null);
            RebuildDatabase = new DelegateCommand<object>(obj=>_ConfirmRebuild());

            if (!DesignerProperties.IsInDesignTool) return;

            CurrentFoodItem = _sampleDescriptions[0];
            CurrentGroup = _samples[0];
        }

        /// <summary>
        ///     Confirm rebuild
        /// </summary>
        private static void _ConfirmRebuild()
        {
            var result = MessageBox.Show("Are you sure you wish to rebuild the entire database?",
                                         "Confirm Database Rebuild", MessageBoxButton.OKCancel);
            
            if (result == MessageBoxResult.OK)
            {
                SterlingService.Current.Navigator.NavigateToBuildView();
            }
        }

        /// <summary>
        ///     Some samples for the designer
        /// </summary>
        private readonly FoodGroup[] _samples = new[]
                                                    {
                                                        new FoodGroup {Id = 1, GroupName = "Meat and Potatoes"},
                                                        new FoodGroup {Id = 2, GroupName = "Chocolate"}
                                                    };

        /// <summary>
        ///     Some sample food descriptions
        /// </summary>
        private readonly FoodDescriptionIndex[] _sampleDescriptions = new[]
                                                                          {
                                                                              new FoodDescriptionIndex
                                                                                  {Id = 1, Description = "Bison Burger"}
                                                                              ,
                                                                              new FoodDescriptionIndex
                                                                                  {Id = 1, Description = "Tofu"}
                                                                          };

        private readonly FoodDescriptionIndex[] _noResults = new[]
                                                                 {
                                                                     new FoodDescriptionIndex
                                                                         {Id = 1, Description = "No results found."}
                                                                 };

        /// <summary>
        ///     Give the bindings it needs
        /// </summary>
        public class FoodDescriptionIndex
        {
            public int Id { get; set; }
            public string Description { get; set; }
        }

        private FoodDescriptionIndex _foodDescriptionIndex;

        /// <summary>
        ///     Synchronizes the current food item
        /// </summary>
        public FoodDescriptionIndex CurrentFoodItem
        {
            get { return _foodDescriptionIndex; }
            set
            {
                _foodDescriptionIndex = value;
                RaisePropertyChanged(()=>CurrentFoodItem);
                if (!DesignerProperties.IsInDesignTool)
                {
                    FoodDescriptionContext.Current.FoodDescriptionId = value == null ? 0 : value.Id;
                }
            }
        }

        /// <summary>
        ///     List of food groups
        /// </summary>
        /// <remarks>
        ///     Because this is a "covered" index, we will make a list of groups instead of
        ///     referencing the value, so Sterling won't have to deserialize from storage
        ///     (indexes are kept in-memory)
        /// </remarks>
        public IEnumerable<FoodGroup> FoodGroups
        {
            get
            {
                return DesignerProperties.IsInDesignTool
                           ? _samples.AsEnumerable()
                           : from fg in
                                 SterlingService.Current.Database.Query<FoodGroup, string, int>(
                                     FoodDatabase.FOOD_GROUP_NAME)
                             select new FoodGroup {Id = fg.Key, GroupName = fg.Index};
            }
        }

        private FoodGroup _currentGroup;

        public FoodGroup CurrentGroup
        {
            get { return _currentGroup; }
            set
            {
                _currentGroup = value;
                RaisePropertyChanged(() => CurrentGroup);
                RaisePropertyChanged(() => SearchResults);
                ClearGroup.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        ///     Clear the group selection
        /// </summary>
        public DelegateCommand<object> ClearGroup { get; private set; }

        public DelegateCommand<object> RebuildDatabase { get; private set; }

        /// <summary>
        ///     Food descriptions
        /// </summary>
        public IEnumerable<FoodDescriptionIndex> SearchResults
        {
            get
            {
                if (DesignerProperties.IsInDesignTool)
                    return _sampleDescriptions.AsEnumerable();

                // filter on group
                if (_currentGroup != null)
                {
                    // group only
                    if (string.IsNullOrEmpty(_searchText) || _searchText.Length < 3)
                    {
                        var query1 = from fg in
                                         SterlingService.Current.Database.Query
                                         <FoodDescription, string, int, int>(
                                             FoodDatabase.FOOD_DESCRIPTION_DESC_GROUP)
                                     where
                                         fg.Index.Item2.Equals(_currentGroup.Id)
                                     select
                                         new FoodDescriptionIndex {Id = fg.Key, Description = fg.Index.Item1};

                        return query1.Count() == 0 ? _noResults.AsEnumerable() : query1;
                    }

                    // group and search text)
                    var query2 = from fg in
                               SterlingService.Current.Database.Query
                               <FoodDescription, string, int, int>(
                                   FoodDatabase.FOOD_DESCRIPTION_DESC_GROUP)
                           where
                               fg.Index.Item2.Equals(_currentGroup.Id) &&
                               fg.Index.Item1.ToUpperInvariant().Contains(_searchText.ToUpperInvariant())
                           select
                               new FoodDescriptionIndex {Id = fg.Key, Description = fg.Index.Item1};
                        
                    return query2.Count() == 0 ? _noResults.AsEnumerable() : query2;
                }

                // not enough search text
                if (string.IsNullOrEmpty(_searchText) || _searchText.Length < 3)
                    return _noResults.AsEnumerable();

                // search text only
                var query3 = from fg in
                           SterlingService.Current.Database.Query
                           <FoodDescription, string, int, int>(
                               FoodDatabase.FOOD_DESCRIPTION_DESC_GROUP)
                       where
                           fg.Index.Item1.ToUpperInvariant().Contains(_searchText.ToUpperInvariant())
                       select
                           new FoodDescriptionIndex { Id = fg.Key, Description = fg.Index.Item1 };
                return query3.Count() == 0 ? _noResults.AsEnumerable() : query3; 
            }
        }
       
        /// <summary>
        ///     Search text
        /// </summary>
        private string _searchText = DesignerProperties.IsInDesignTool ? "Sample Search" : string.Empty;

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                RaisePropertyChanged(() => SearchText);
                RaisePropertyChanged(() => SearchResults);                
            }
        }
    }
}