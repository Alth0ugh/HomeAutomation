using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HomeAutomationUWP.Helper_classes;

namespace HomeAutomationUWP.ViewModels
{
    public class LightControlModel : BindableBase
    {
        private int _colorTemperature;
        public int ColorTemperature
        {
            get
            {
                return _colorTemperature;
            }
            set
            {
                _colorTemperature = value;
                NotifyPropertyChanged("ColorTemperature");
            }
        }

        private ICommand _searchCommand;
        public ICommand SearchCommand
        {
            get
            {
                return _searchCommand;
            }
            set
            {
                _searchCommand = value;
                NotifyPropertyChanged("SearchCommand");
            }
        }

        private ICommand _openDeviceSelectorCommand;
        public ICommand OpenDeviceSelectorCommand
        {
            get
            {
                return _openDeviceSelectorCommand;
            }
            set
            {
                _openDeviceSelectorCommand = value;
                NotifyPropertyChanged("OpenDeviceSelector");
            }
        }

        private bool _isSearching;
        public bool IsSearching
        {
            get
            {
                return _isSearching;
            }
            set
            {
                _isSearching = value;
                NotifyPropertyChanged("IsSearching");
            }
        }

        private ObservableCollection<YeelightDevice> _yeelightDevices;
        public ObservableCollection<YeelightDevice> YeelightDevices
        {
            get
            {
                if (_yeelightDevices == null)
                {
                    _yeelightDevices = new ObservableCollection<YeelightDevice>();
                }
                return _yeelightDevices;
            }
            set
            {
                _yeelightDevices = value;
                NotifyPropertyChanged("YeelightDevices");
            }
        }

        private bool _isSearchOpen;
        public bool IsSearchOpen
        {
            get
            {
                return _isSearchOpen;
            }
            set
            {
                _isSearchOpen = value;
                NotifyPropertyChanged("IsSearchOpen");
            }
        }

        public LightControlModel()
        {
            SetCommands();
        }

        private void SetCommands()
        {
            SearchCommand = new RelayCommand(SearchForDevices);
            OpenDeviceSelectorCommand = new RelayCommand(OpenDeviceSelector);
        }

        private async void OpenDeviceSelector(object obj)
        {
            /*
            var devices = await YeelightDevice.FindDevices();
            foreach (var device in devices)
            {
                Debug.WriteLine("Device: " + device.IpAddresss);
                Debug.WriteLine("Port: " + device.Port);
                Debug.WriteLine("");
            }*/
            IsSearching = IsSearching ? false : true;
            IsSearchOpen = IsSearchOpen ? false : true;
        }

        private void SearchForDevices(object obj)
        {
            IsSearching = IsSearching ? false : true;
        }
    }
}
