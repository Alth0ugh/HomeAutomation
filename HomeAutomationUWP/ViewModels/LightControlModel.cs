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
        private double _colorTemperature = 1700;
        public double ColorTemperature
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

        private double _brightness = 0;
        public double Brightness
        {
            get
            {
                return _brightness;
            }
            set
            {
                _brightness = value;
                ConnectedDevice.SetBrightness((int)_brightness);
                NotifyPropertyChanged("Brightness");
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

        private ICommand _connectCommand;
        public ICommand ConnectCommand
        {
            get
            {
                return _connectCommand;
            }
            set
            {
                _connectCommand = value;
                NotifyPropertyChanged("ConnectCommand");
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

        private ObservableCollection<YeelightDeviceCharacteristic> _yeelightDevices = new ObservableCollection<YeelightDeviceCharacteristic>();
        public ObservableCollection<YeelightDeviceCharacteristic> YeelightDevices
        {
            get
            {
                if (_yeelightDevices == null)
                {
                    _yeelightDevices = new ObservableCollection<YeelightDeviceCharacteristic>();
                }
                return _yeelightDevices;
            }
            set
            {
                _yeelightDevices = value;
                NotifyPropertyChanged("YeelightDevices");
            }
        }

        private YeelightDeviceCharacteristic _selectedItem;
        public YeelightDeviceCharacteristic SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                if (_selectedItem != null)
                {
                    _selectedItem.ConnectButtonVisibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
                _selectedItem = value;
                NotifyPropertyChanged("SelectedItem");
                _selectedItem.ConnectButtonVisibility = Windows.UI.Xaml.Visibility.Visible;
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

        private bool _isConnecting;
        public bool IsConnecting
        {
            get
            {
                return _isConnecting;
            }
            set
            {
                _isConnecting = value;
                NotifyPropertyChanged("IsConnecting");
            }
        }

        private YeelightDevice _connectedDevice;
        public YeelightDevice ConnectedDevice
        {
            get
            {
                return _connectedDevice;
            }
            set
            {
                _connectedDevice = value;
                NotifyPropertyChanged("ConnectedDevice");
            }
        }

        private ushort _connectingState;
        public ushort ConnectingState
        {
            get
            {
                return _connectingState;
            }
            set
            {
                _connectingState = value;
                NotifyPropertyChanged("ConnectingState");
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
            ConnectCommand = new RelayCommand(ConnectToLight);
        }

        private async void ConnectToLight(object obj)
        {
            var deviceCharacteristic = obj as YeelightDeviceCharacteristic;
            if (deviceCharacteristic == null)
            {
                return;
            }

            ConnectedDevice = await YeelightDevice.Connect(deviceCharacteristic);
            ConnectedDevice.SetPower(true);

            //ConnectingState = ConnectedDevice.Connected ? (ushort)1 : (ushort)2;
        }

        private async void OpenDeviceSelector(object obj)
        {
            IsSearchOpen = true;
            IsSearching = true;
            var devices = await YeelightDevice.FindDevices();
            IsSearching = false;
            foreach (var device in devices)
            {
                YeelightDevices.Add(device);
            }
        }

        private void SearchForDevices(object obj)
        {
            IsSearching = IsSearching ? false : true;
        }
    }
}
