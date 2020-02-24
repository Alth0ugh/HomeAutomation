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
    public class PoolMenuModel : BindableBase, INavigateBackAction, INavigateAction
    {
        private System.Timers.Timer _poolTimer;
        private ESP8266 _client;
        private int _poolPower;
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

        private bool _isDeviceSelectorOpen = false;
        public bool IsDeviceSelectorOpen
        {
            get
            {
                return _isDeviceSelectorOpen;
            }
            set
            {
                _isDeviceSelectorOpen = value;
                NotifyPropertyChanged("IsDeviceSelectorOpen");
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
            _client = new ESP8266();
            _client.OnConnected += new ESP8266.OnConnectedHandler(OnESPConnected);
            _client.OnDisconnected += new ESP8266.OnDisconnectedHandler(OnESPDisconnected);
            Task.Run(new Action(_client.Listen));
            
            _poolTimer = new System.Timers.Timer(60000);
            _poolTimer.Elapsed += new ElapsedEventHandler(CheckPoolTime);
            _poolTimer.Start();
        }

        private void CheckPoolTime(object sender, ElapsedEventArgs e)
        {
            _poolTimer.Stop();
            var hour = DateTime.Now.Hour + 1;
            var half = ListOfTimeSelectors.Count / 2;
            int lIndex = 0;
            int HIndex = ListOfTimeSelectors.Count - 1;
            var hourFromSelector = ListOfTimeSelectors[half].FromTime;
            int numberIndex = -1;

            int previousStartingTimeIndex;
            int nextStartingTimeIndex;

            while (lIndex <= HIndex && numberIndex == -1)
            {
                if (hourFromSelector == hour)
                {
                    SetESPStatus(true);
                    return;
                }

                if (hour < hourFromSelector)
                {
                    HIndex = half - 1;
                    half = ((HIndex - lIndex) / 2) + lIndex;
                }
                else
                {
                    lIndex = half + 1;
                    half = ((HIndex - lIndex) / 2) + lIndex;
                }

                hourFromSelector = ListOfTimeSelectors[half].FromTime;
            }

            if (HIndex != ListOfTimeSelectors.Count - 1)
            {
                if (PoolPower == 0)
                {
                    if ((ListOfTimeSelectors[lIndex].FromTime < hour &&
                        ListOfTimeSelectors[lIndex].ToTime > hour) ||
                        (ListOfTimeSelectors[HIndex].FromTime < hour &&
                        ListOfTimeSelectors[HIndex].ToTime > hour))
                    {
                        SetESPStatus(true);
                    }
                }
                else if (PoolPower == 1)
                {
                    if ((ListOfTimeSelectors[lIndex].FromTime > hour ||
                        ListOfTimeSelectors[lIndex].ToTime < hour) ||
                        (ListOfTimeSelectors[HIndex].FromTime > hour &&
                        ListOfTimeSelectors[HIndex].ToTime < hour))
                    {
                        SetESPStatus(false);
                    }
                }
            }
            else
            {
                if (PoolPower == 0)
                {
                    if (ListOfTimeSelectors[HIndex].ToTime < hour && ListOfTimeSelectors[HIndex].ToTime > hour)
                    {
                        SetESPStatus(true);
                    }
                }
                else if (PoolPower == 1)
                {
                    if (ListOfTimeSelectors[HIndex].ToTime > hour && ListOfTimeSelectors[HIndex].ToTime < hour)
                    {
                        SetESPStatus(false);
                    }
                }
            }
            _poolTimer.Start();
        }

        private void OnESPDisconnected()
        {
            PoolPower = -1;
        }

        private void ReconnectESP(object obj)
        {
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
            OnOffCommand = new AsyncRelayCommand(SetESPStatus);
            AddTimeCommand = new RelayCommand(AddTimeEntry);
            ReconnectCommand = new RelayCommand(ReconnectESP);
        }

        /// <summary>
        /// Serializes the time intervals and saves the data int a file.
        /// </summary>
        /// <param name="obj"></param>
        private async void Serialize(object obj)
        {
            ConvertIntervals();
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile file = await storageFolder.CreateFileAsync("test.txt", Windows.Storage.CreationCollisionOption.ReplaceExisting);

            var serializer = new DataContractJsonSerializer(typeof(ObservableCollection<TimeSelectorCharacteristic>));

            using (var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
            {
                using (var outputStream = stream.GetOutputStreamAt(0))
                {
                    serializer.WriteObject(outputStream.AsStreamForWrite(), ListOfTimeSelectors);
                }
            }
        }

        /// <summary>
        /// Sorts intervals.
        /// </summary>
        /// <returns>Sorted list.</returns>
        private ObservableCollection<TimeSelectorCharacteristic> SortIntervals()
        {
            List<TimeSelectorCharacteristic> sorted = ListOfTimeSelectors.OrderBy(o => o.FromTime).ToList();
            ObservableCollection<TimeSelectorCharacteristic> newArray = new ObservableCollection<TimeSelectorCharacteristic>();

            foreach (var item in sorted)
            {
                newArray.Add(item);
            }
            return newArray;
        }

        /// <summary>
        /// Merges overlapping intervals.
        /// </summary>
        private void ConvertIntervals()
        {
            var oldArray = SortIntervals();
            ObservableCollection<TimeSelectorCharacteristic> newArray = new ObservableCollection<TimeSelectorCharacteristic>();
            if (oldArray.Count == 0)
            {
                return;
            }

            newArray.Add(oldArray[0]);
            for (int i = 1; i < ListOfTimeSelectors.Count; i++)
            {
                var top = newArray[0];
                if (oldArray[i].FromTime > top.ToTime)
                {
                    newArray.Insert(0, oldArray[i]);
                }
                else if (oldArray[i].ToTime > top.ToTime)
                {
                    top.ToTime = oldArray[i].ToTime;
                }
                /*
                int j = i + 1;
                //for (int j = i + 1; j < ListOfTimeSelectors.Count; j++)
                while(j < ListOfTimeSelectors.Count)
                {
                    if (ListOfTimeSelectors[j].FromTime == ListOfTimeSelectors[i].FromTime && ListOfTimeSelectors[j].ToTime == ListOfTimeSelectors[i].ToTime)
                    {
                        ListOfTimeSelectors.RemoveAt(j);
                    }
                    else if (ListOfTimeSelectors[j].FromTime <= ListOfTimeSelectors[i].FromTime && ListOfTimeSelectors[j].ToTime >= ListOfTimeSelectors[i].FromTime)
                    {
                        ListOfTimeSelectors[i].FromTime = ListOfTimeSelectors[j].FromTime;
                        ListOfTimeSelectors.RemoveAt(j);
                    }
                    else if (ListOfTimeSelectors[j].ToTime >= ListOfTimeSelectors[i].ToTime && ListOfTimeSelectors[j].FromTime <= ListOfTimeSelectors[i].ToTime)
                    {
                        ListOfTimeSelectors[i].ToTime = ListOfTimeSelectors[j].ToTime;
                        ListOfTimeSelectors.RemoveAt(j);
                    }
                    else if (ListOfTimeSelectors[j].FromTime < ListOfTimeSelectors[i].FromTime && ListOfTimeSelectors[j].ToTime > ListOfTimeSelectors[i].ToTime)
                    {
                        ListOfTimeSelectors.RemoveAt(i);
                    }
                    else
                    {
                        j++;
                    }
                }
                */
            }
            ListOfTimeSelectors = new ObservableCollection<TimeSelectorCharacteristic>(newArray.Reverse());
        }

        /// <summary>
        /// Adds an interval to collection.
        /// </summary>
        /// <param name="obj"></param>
        private void AddTimeEntry(object obj)
        {
            ListOfTimeSelectors.Add(new TimeSelectorCharacteristic());
        }

        private async Task UpdatePoolPower()
        {
            int power = -1;
            try
            {
                power = await Task.Run(_client.GetPoolStatus);
            }
            catch { }
            finally
            {
                PoolPower = power;
            }
        }

        private async Task SetESPStatus(object obj)
        {
            bool newStatus;
            if (obj != null && obj is bool)
            {
                newStatus = (bool)obj;

                if (PoolPower == 0 && newStatus == true)
                {
                    try
                    {
                        await Task.Run(new Action(_client.TurnOn));
                    }
                    catch { }
                }
                else if (PoolPower == 1 && newStatus == false)
                {
                    try
                    {
                        await Task.Run(new Action(_client.TurnOff));
                    }
                    catch { }
                }
            }
            await UpdatePoolPower();
            if (PoolPower == 1)
            {
                try
                {
                    await Task.Run(new Action(_client.TurnOff));
                }
                catch
                { }
            }
            else if (PoolPower == 0)
            {
                try
                {
                    await Task.Run(new Action(_client.TurnOn));
                }
                catch
                { }
            }
            await UpdatePoolPower();
        }

        public void OnNavigateBackAction(object obj)
        {
            Debug.WriteLine("serializing");
            Serialize(null);
        }

        public void Test(object obj)
        {
            var a = new ObservableCollection<TimeSelectorCharacteristic>();

            a.Add(new TimeSelectorCharacteristic() { FromTime = 10, ToTime = 20 });
            a.Add(new TimeSelectorCharacteristic() { FromTime = 1, ToTime = 3 });

            ListOfTimeSelectors = a;
        }

        public async Task NavigatedTo()
        {
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var file = await storageFolder.OpenStreamForReadAsync("test.txt");
            
            var serializer = new DataContractJsonSerializer(typeof(ObservableCollection<TimeSelectorCharacteristic>));
             ListOfTimeSelectors = (ObservableCollection<TimeSelectorCharacteristic>)serializer.ReadObject(file);
        }
    }
}