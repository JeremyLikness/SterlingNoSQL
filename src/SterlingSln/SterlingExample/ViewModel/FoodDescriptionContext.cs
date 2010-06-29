using SterlingExample.Model;

namespace SterlingExample.ViewModel
{
    /// <summary>
    ///     Context of "current" food description 
    /// </summary>
    public class FoodDescriptionContext : BaseNotify 
    {
        public static FoodDescriptionContext Current = new FoodDescriptionContext();

        public FoodDescription CurrentFoodDescription { get; private set; }

        private int _foodDescriptionId; 

        public int FoodDescriptionId
        {
            get { return _foodDescriptionId; }
            set
            {
                _foodDescriptionId = value;
                CurrentFoodDescription = SterlingService.Current.Database.Load<FoodDescription>(value);
                RaisePropertyChanged(()=>CurrentFoodDescription);
                RaisePropertyChanged(()=>FoodDescriptionId);
            }
        }

    }
}
