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
        private System.Timers.Timer _reconnectTimer;
        private TcpListener _listener;
        private SslStream _stream;
        private TcpClient _client;
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

        public ICommand OnOffCommand { get; set; }
        public ICommand AddTimeCommand { get; set; }
        public ICommand SerializeCommand { get; set; }

        public PoolMenuModel()
        {
            PoolPower = -1;

            SetCommands();
            ListOfTimeSelectors.Add(new TimeSelectorCharacteristic() { FromTime = 1, ToTime = 3 });
            ListOfTimeSelectors.Add(new TimeSelectorCharacteristic() { FromTime = 4, ToTime = 5 });
            ListOfTimeSelectors.Add(new TimeSelectorCharacteristic() { FromTime = 2, ToTime = 8 });
            ListOfTimeSelectors.Add(new TimeSelectorCharacteristic() { FromTime = 3, ToTime = 4 });

            _listener = new TcpListener(443);
            _listener.Start();
            Task.Run(new Action(HandleESPConnection));

            _reconnectTimer = new System.Timers.Timer(5000);
            _reconnectTimer.Elapsed += new ElapsedEventHandler(Reconnect);
        }

        private void SetCommands()
        {
            OnOffCommand = new RelayCommand(SetESPStatus);
            AddTimeCommand = new RelayCommand(AddTimeEntry);
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
            ListOfTimeSelectors = newArray;
        }

        /// <summary>
        /// Adds an interval to collection.
        /// </summary>
        /// <param name="obj"></param>
        private void AddTimeEntry(object obj)
        {
            ListOfTimeSelectors.Add(new TimeSelectorCharacteristic());
        }

        private void SetESPStatus(object obj)
        {
            GetPoolStatus();
            string message;

            if (PoolPower == 1)
            {
                message = "turnOff\n";
                _stream.Write(ASCIIEncoding.ASCII.GetBytes(message));
            }
            else if (PoolPower == 0)
            {
                message = "turnOn\n";
                _stream.Write(ASCIIEncoding.ASCII.GetBytes(message));
            }
            GetPoolStatus();
        }

        private async Task GetPoolStatus()
        {
            string message = "getPoolStatus\n";
            _stream.Write(ASCIIEncoding.ASCII.GetBytes(message));

            string response = string.Empty;
            char newCharacter;

            do
            {
                newCharacter = (char)_stream.ReadByte();
                if (newCharacter != '\n')
                {
                    response += newCharacter;
                }
            } while (newCharacter != '\n');

            if (response == "true")
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,() =>
                    {
                        PoolPower = 1;
                    }
                );
            }
            else
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    PoolPower = 0;
                }
                );
            }
            
        }

        private void Reconnect(object sender, ElapsedEventArgs e)
        {
            HandleESPConnection();
        }

        public async void HandleESPConnection()
        {
            //if (_listener.Pending())
            //{
                _client = _listener.AcceptTcpClient();
                _stream = new SslStream(_client.GetStream(), false);

                Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

                var certificate = new X509Certificate2(storageFolder.Path + @"\Server.p12");
                _stream.AuthenticateAsServer(certificate, clientCertificateRequired: false, enabledSslProtocols: System.Security.Authentication.SslProtocols.Tls11, checkCertificateRevocation: false);
                _reconnectTimer.Stop();

                await GetPoolStatus();
                Debug.WriteLine("PoolPower = " + PoolPower);
            //}
        }

        public void OnNavigateBackAction(object obj)
        {
            Debug.WriteLine("serializing");
            Serialize(null);
        }
    }
}