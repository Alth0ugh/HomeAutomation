using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HomeAutomationUWP.Controls;
using Windows.UI.Xaml;

namespace HomeAutomationUWP.ViewModels
{
    public class PoolMenuModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private bool _poolPower;
        public bool PoolPower
        {
            get
            {
                return _poolPower;
            }
            set
            {
                _poolPower = value;
                NotifyPropertyChanged("PoolPower");
            }
        }

        //public static DependencyProperty ListOfTimeSelectorsDependencyProperty = DependencyProperty.Register("ListOfTimeSelectors", typeof(ObservableCollection<TimeSelectorCharacteristic>), typeof(PoolMenuModel), new PropertyMetadata(new ObservableCollection<TimeSelectorCharacteristic>()));
        private ObservableCollection<TimeSelectorCharacteristic> _listOfTimeSelectors = new ObservableCollection<TimeSelectorCharacteristic>() { new TimeSelectorCharacteristic { FromTime = 0, ToTime = 1 } };
        public ObservableCollection<TimeSelectorCharacteristic> ListOfTimeSelectors
        {
            get
            {
                return _listOfTimeSelectors;
            }
            set
            {
                _listOfTimeSelectors = value;
                NotifyPropertyChanged("ListOfTimeSelectors");
            }
        }

        private ushort _fromTime = 1;
        public ushort FromTime
        {
            get
            {
                return _fromTime;
            }
            set
            {
                _fromTime = value;
                Debug.WriteLine("FromTime changed: " + value);
                NotifyPropertyChanged("FromTime");
            }
        }

        public PoolMenuModel()
        {
            PoolPower = false;
            //ListOfTimeSelectors = new ObservableCollection<TimeSelectorCharacteristic>();

            //ListOfTimeSelectors.Add(new TimeSelectorCharacteristic { From = 1, To = 2 });
            //ListOfTimeSelectors.Add(new TimeSelectorCharacteristic() { From = 1, To = 5});
        }


        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
