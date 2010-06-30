using System;
using System.ComponentModel;
using SterlingExample.WindowsPhone.Database;

namespace SterlingExample.WindowsPhone.ViewModels
{
    public class ItemViewModel : INotifyPropertyChanged
    {
        private bool _loaded = false;

        private readonly object _lock = new object();

        private string _lineOne;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public string LineOne
        {
            get
            {
                return _lineOne;
            }
            set
            {
                _lineOne = value;
                NotifyPropertyChanged("LineOne");
            }
        }

        private string _lineTwo;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public string LineTwo
        {
            get
            {
                return _lineTwo;
            }
            set
            {
                _lineTwo = value;
                NotifyPropertyChanged("LineTwo");
            }
        }

        private string _lineThree;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// Lazy load from Sterling on first access
        /// </summary>
        /// <returns></returns>
        public string LineThree
        {
            get
            {
                return _lineThree;
            }
            set
            {
                _lineThree = value;
                NotifyPropertyChanged("LineThree");
            }
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