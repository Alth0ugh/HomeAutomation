using System;
using System.Collections.Generic;
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

        public LightControlModel()
        {
            SetCommands();
        }

        private void SetCommands()
        {
            SearchCommand = new RelayCommand(SearchForDevices);
        }

        private async void SearchForDevices(object obj)
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
        }
    }
}
