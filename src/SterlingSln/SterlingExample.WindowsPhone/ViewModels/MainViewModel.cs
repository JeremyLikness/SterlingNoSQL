using System;
using System.ComponentModel;
using System.Linq;
using System.Collections.ObjectModel;
using SterlingExample.WindowsPhone.Database;

namespace SterlingExample.WindowsPhone.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            var databaseInstance = DatabaseService.Current.Database;
            ((PhoneDatabase)databaseInstance).CheckData();
            var list = (from index in databaseInstance.Query<ItemViewModel, string, string, string>(PhoneDatabase.INDEX)
                     select new ItemViewModel {LineOne = index.Key, LineTwo = index.Index.Item1, LineThree = index.Index.Item2 }).ToList();
            Items =
                new ObservableCollection<ItemViewModel>();
            foreach(var item in list)
            {
                Items.Add(item);
            }
        }

        public ObservableCollection<ItemViewModel> Items { get; private set; }

        private string _sampleProperty = "Sample Runtime Property Value";
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding
        /// </summary>
        /// <returns></returns>
        public string SampleProperty
        {
            get
            {
                return _sampleProperty;
            }
            set
            {
                _sampleProperty = value;
                NotifyPropertyChanged("SampleProperty");
            }
        }

        /// <summary> Sample ViewModel method; this method is invoked by a Behavior that is associated with it in the View</summary>
        public void SampleMethod()
        {
            SampleProperty = "SampleMethod invoked.";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}