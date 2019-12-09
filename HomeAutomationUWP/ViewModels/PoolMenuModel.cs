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
        }

        private void SetCommands()
        {
            OnOffCommand = new RelayCommand(SetESPStatus);
            AddTimeCommand = new RelayCommand(AddTimeEntry);
            SerializeCommand = new RelayCommand(Serialize);
        }

        private void Serialize(object obj)
        {
            var serializer = new DataContractJsonSerializer(typeof(ObservableCollection<TimeSelectorCharacteristic>));
            var memoryStream = new MemoryStream();
            serializer.WriteObject(memoryStream, ListOfTimeSelectors);
            memoryStream.Position = 0;
            var reader = new StreamReader(memoryStream);
            Debug.WriteLine(reader.ReadToEnd());
            /*Task.Run(() =>
            {
                var fileStream = new FileStream((Windows.ApplicationModel.Package.Current.InstalledLocation)., FileMode.Create);
                memoryStream.WriteTo(fileStream);
            });*/
        }

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
