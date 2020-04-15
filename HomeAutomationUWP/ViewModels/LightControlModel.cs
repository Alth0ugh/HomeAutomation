using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HomeAutomationUWP.Helper_classes;
using HomeAutomationUWP.Loggers;
using HomeAutomationUWP.Helper_interfaces;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Core;

namespace HomeAutomationUWP.ViewModels
{
    public class LightControlModel : BindableBase, INavigateAction
    {
        private double _colorTemperature = 2700;
        public double ColorTemperature
        {
            get
            {
                return _colorTemperature;
            }
            set
            {
                if (Math.Floor(value / 500) * 500 == _colorTemperature)
                {
                    return;
                }

                _colorTemperature = Math.Floor(value / 500) * 500;
                ConnectedDevice?.SetColorTemperature((int)_colorTemperature);
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
                if (value != 0 && _brightness == 0)
                {
                    ConnectedDevice?.SetPower(true);
                }

                if (value == 1)
                {
                    ConnectedDevice?.SetBrightness(1);
                    return;
                }
                else if (Math.Floor(value / 10) * 10 == _brightness && value != 0)
                {
                    return;
                }
                else if (value == 0)
                {
                    ConnectedDevice?.SetPower(false);
                    ConnectedDevice?.SetBrightness(1);
                }

                
                _brightness = Math.Floor(value / 10) * 10;
                ConnectedDevice?.SetBrightness((int)_brightness);
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

        private ICommand _changeSceneCommand;
        public ICommand ChangeSceneCommand
        {
            get
            {
                return _changeSceneCommand;
            }
            set
            {
                _changeSceneCommand = value;
                NotifyPropertyChanged("ChangeSceneCommand");
            }
        }

        private ICommand _disconnectLightCommand;
        public ICommand DisconnectLightCommand
        {
            get
            {
                return _disconnectLightCommand;
            }
            set
            {
                _disconnectLightCommand = value;
                NotifyPropertyChanged("DisconnectLightCommand");
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

        private bool _controlsEnabled;
        public bool ControlsEnabled
        {
            get
            {
                return _controlsEnabled;
            }
            set
            {
                _controlsEnabled = value;
                NotifyPropertyChanged("ControlsEnabled");
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
                    _selectedItem.ConnectButtonVisibility = Visibility.Collapsed;
                }
                _selectedItem = value;
                if (_selectedItem != null)
                {
                    _selectedItem.ConnectButtonVisibility = Visibility.Visible;
                }
                NotifyPropertyChanged("SelectedItem");
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
                if (!value)
                {
                    YeelightDevices.Clear();
                }
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
            SearchCommand = new AsyncRelayCommand(SearchForDevices);
            OpenDeviceSelectorCommand = new AsyncRelayCommand(OpenDeviceSelector);
            ConnectCommand = new RelayCommand(ConnectToLight);
            ChangeSceneCommand = new RelayCommand(SetLightMode);
            DisconnectLightCommand = new RelayCommand(DisconnectLight);
        }

        private void DisconnectLight(object sender)
        {
            if (ConnectedDevice != null)
            {
                ConnectedDevice.Disconnect();
                ConnectedDevice = null;
                ControlsEnabled = false;

            }
        }

        private void SetLightMode(object sender)
        {
            var button = sender as Button;
            if (button == null)
            {
                return;
            }

            var tag = button.Tag;

            switch (tag)
            {
                case (int)LightModes.NightMode:
                    ConnectedDevice.SetScene(LightModes.NightMode);
                    break;
                case (int)LightModes.DayMode:
                    ConnectedDevice.SetScene(LightModes.DayMode);
                    break;
                default:
                    break;
            }
        }

        private async void ConnectToLight(object obj)
        {
                /*
            if (obj is Task<YeelightDeviceCharacteristic>)
            {
                previousCharacteristic = await (obj as Task<YeelightDeviceCharacteristic>);
                var onlineDevices = await YeelightDevice.FindDevices();
                YeelightDeviceCharacteristic editedCharacteristic;

                if (onlineDevices.Exists(o => o.IpAddress == ((YeelightDeviceCharacteristic)previousCharacteristic).IpAddress))
                {
                    var device = onlineDevices.Find(o => o.IpAddress == ((YeelightDeviceCharacteristic)previousCharacteristic).IpAddress);
                    editedCharacteristic = new YeelightDeviceCharacteristic(((YeelightDeviceCharacteristic)previousCharacteristic).IpAddress, device.Port, device.AvaliableMethods);
                    ConnectedDevice = await YeelightDevice.Connect(editedCharacteristic);
                    _brightness = device.Brightness;
                    NotifyPropertyChanged("Brightness");
                    _colorTemperature = device.ColorTemperature;
                    NotifyPropertyChanged("ColorTemperature");
                }
            }
            */
            if (obj is YeelightDeviceCharacteristic)
            {
                var deviceCharacteristic = obj as YeelightDeviceCharacteristic;

                ConnectedDevice = await YeelightDevice.Connect(deviceCharacteristic);
                ControlsEnabled = (ConnectedDevice.Connected) ? true : false;

                if (ConnectedDevice.DeviceCharacteristic.Power == "on")
                {
                    _brightness = deviceCharacteristic.Brightness;
                }
                else
                {
                    _brightness = 0;
                }
                NotifyPropertyChanged("Brightness");
                _colorTemperature = deviceCharacteristic.ColorTemperature;
                NotifyPropertyChanged("ColorTemperature");
                await ConfigLogger.Log(ConfigType.LightConfig, deviceCharacteristic);
            }
        }

        private async Task OpenDeviceSelector(object obj)
        {
            await Task.Run(async () =>
            {
                IsSearchOpen = true;
                IsSearching = true;
                var devices = await YeelightDevice.FindDevices();
                IsSearching = false;

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    YeelightDevices.Clear();
                });

                foreach (var device in devices)
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        YeelightDevices.Add(device);
                    });
                }
            });
        }

        private async Task SearchForDevices(object obj)
        {
            await Task.Run(async () =>
            {
                if (IsSearching)
                {
                    return;
                }

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    YeelightDevices.Clear();
                });

                IsSearching = true;
                var devices = await YeelightDevice.FindDevices();
                IsSearching = false;
                foreach (var device in devices)
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        
                         YeelightDevices.Add(device);
                    });
                }
            });
        }

        public async void NavigatedTo()
        {
            var onlineDevices = await YeelightDevice.FindDevices();
            var previousCharacteristic = await ConfigReader.ReadEntireConfigAsync<YeelightDeviceCharacteristic>(ConfigType.LightConfig);

            if (previousCharacteristic == null)
            {
                return;
            }

            if (onlineDevices.Exists(o => o.IpAddress == previousCharacteristic.IpAddress))
            {
                ConnectToLight(onlineDevices.Find(o => o.IpAddress == previousCharacteristic.IpAddress));
            }
        }
    }

    public enum LightModes
    {
        DayMode = 1,
        NightMode = 2
    }
}
