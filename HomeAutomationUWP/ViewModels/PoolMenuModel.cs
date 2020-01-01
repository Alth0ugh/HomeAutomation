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
using System.Runtime.Serialization.Json;
using System.IO;
using System.Threading;
using Windows.Storage.Streams;
using Windows.Storage.Pickers;

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
            PoolPower = false;

            SetCommands();
            ListOfTimeSelectors.Add(new TimeSelectorCharacteristic() { FromTime = 1, ToTime = 3 });
            ListOfTimeSelectors.Add(new TimeSelectorCharacteristic() { FromTime = 4, ToTime = 5 });
            ListOfTimeSelectors.Add(new TimeSelectorCharacteristic() { FromTime = 2, ToTime = 8 });
            ListOfTimeSelectors.Add(new TimeSelectorCharacteristic() { FromTime = 3, ToTime = 4 });
        }

        private void SetCommands()
        {
            OnOffCommand = new RelayCommand(SetESPStatus);
            AddTimeCommand = new RelayCommand(AddTimeEntry);
            SerializeCommand = new RelayCommand(Serialize);
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
            var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
            var serializer = new DataContractJsonSerializer(typeof(ObservableCollection<TimeSelectorCharacteristic>));
            var memoryStream = new MemoryStream();
            serializer.WriteObject(stream.GetOutputStreamAt(0).AsStreamForWrite(), ListOfTimeSelectors);
            memoryStream.Position = 0;
            var reader = new StreamReader(memoryStream);
            stream.Dispose();
            Debug.WriteLine(reader.ReadToEnd());
            /*Task.Run(() =>
            {
                var fileStream = new FileStream((Windows.ApplicationModel.Package.Current.InstalledLocation)., FileMode.Create);
                memoryStream.WriteTo(fileStream);
            });*/
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
            if (PoolPower)
            {
                PoolPower = false;
            }
            else
            {
                PoolPower = true;
            }
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
