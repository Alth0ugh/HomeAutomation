using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HomeAutomationUWP.Controls;
using Windows.UI.Xaml;
using HomeAutomationUWP.Helper_classes;
using HomeAutomationUWP.Helper_interfaces;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using Windows.Storage.Streams;
using System.Security.Cryptography.X509Certificates;
using Windows.UI.Core;

namespace HomeAutomationUWP.ViewModels
{
    public class PoolMenuModel : BindableBase, INavigateBackAction
    {
        private PoolControler _client;
        private int _poolPower;
        /// <summary>
        /// Indicates the status of the pool. 1 - ON, 0 - OFF, -1 - ERROR
        /// </summary>
        public int PoolPower
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

        private ObservableCollection<TimeSelectorCharacteristic> _listOfTimeSelectors = new ObservableCollection<TimeSelectorCharacteristic>();
        /// <summary>
        /// Lists the times when the pool should be on.
        /// </summary>
        public ObservableCollection<TimeSelectorCharacteristic> ListOfTimeSelectors
        {
            get
            {
                if (_listOfTimeSelectors == null)
                {
                    return new ObservableCollection<TimeSelectorCharacteristic>();
                }
                return _listOfTimeSelectors;
            }
            set
            {
                _listOfTimeSelectors = value;
                NotifyPropertyChanged("ListOfTimeSelectors");
            }
        }

        public ICommand OnOffCommand { get; set; }
        public ICommand AddTimeCommand { get; set; }
        public ICommand SerializeCommand { get; set; }
        public ICommand ReconnectCommand { get; set; }

        public PoolMenuModel()
        {
            PoolPower = -1;

            SetCommands();
            /*
            ListOfTimeSelectors.Add(new TimeSelectorCharacteristic() { FromTime = 1, ToTime = 3 });
            ListOfTimeSelectors.Add(new TimeSelectorCharacteristic() { FromTime = 4, ToTime = 5 });
            ListOfTimeSelectors.Add(new TimeSelectorCharacteristic() { FromTime = 2, ToTime = 8 });
            ListOfTimeSelectors.Add(new TimeSelectorCharacteristic() { FromTime = 3, ToTime = 4 });
            */
            _client = PoolChecker.PoolClient;
            _client.OnConnected += new ESP8266.OnConnectedHandler(OnESPConnected);
            _client.OnDisconnected += new ESP8266.OnDisconnectedHandler(OnESPDisconnected);
            ListOfTimeSelectors = new ObservableCollection<TimeSelectorCharacteristic>(PoolChecker.PoolTimes);
            PoolChecker.OnPoolStateChanged += new PoolChecker.OnPoolStateChangedEventHandler(UpdatePoolState);
        }

        private void UpdatePoolState()
        {
            PoolPower = PoolChecker.PoolPower;
        }

        private void OnESPDisconnected()
        {
            PoolPower = -1;
        }

        private void ReconnectESP(object obj)
        {
            if (_client == null)
            {
                return;
            }
            Task.Run(new Action(_client.Listen));
        }

        private async void OnESPConnected()
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    PoolPower = await _client.GetPoolStatus();
                }
                catch { }
            });
        }

        private void SetCommands()
        {
            OnOffCommand = new AsyncRelayCommand(PoolChecker.SetESPStatus);
            AddTimeCommand = new RelayCommand(AddTimeEntry);
            ReconnectCommand = new RelayCommand(ReconnectESP);
        }

        
        /// <summary>
        /// Adds an interval to collection.
        /// </summary>
        /// <param name="obj"></param>
        private void AddTimeEntry(object obj)
        {
            var newTime = new TimeSelectorCharacteristic();
            ListOfTimeSelectors.Add(newTime);
            PoolChecker.PoolTimes.Add(newTime);
        }

        public void OnNavigateBackAction(object obj)
        {
            Debug.WriteLine("serializing");
            PoolChecker.Serialize(null);
        }
    }
}