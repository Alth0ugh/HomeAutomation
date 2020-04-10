using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using HomeAutomationUWP.Controls;

namespace HomeAutomationUWP.Helper_classes
{
    public static class PoolChecker
    {
        public static PoolControler PoolClient { get; set; }
        private static bool _isESPConnected = false;
        private static Timer _poolTimer;

        public static List<TimeSelectorCharacteristic> PoolTimes { get; set; }

        public static int PoolPower { get; set; }

        public delegate void OnPoolStateChangedEventHandler();
        /// <summary>
        /// Occurs when pool status changes.
        /// </summary>
        public static event OnPoolStateChangedEventHandler OnPoolStateChanged;

        /// <summary>
        /// Initializes PoolChecker.
        /// </summary>
        public static void Init()
        {
            PoolClient = new PoolControler();
            PoolClient.OnConnected += new ESP8266.OnConnectedHandler(OnESPConnected);
            PoolClient.OnDisconnected += new ESP8266.OnDisconnectedHandler(OnESPDisconnected);
            _poolTimer = new Timer(6000);
            _poolTimer.Elapsed += new ElapsedEventHandler(CheckPoolTime);

            Task.Run(async () =>
            {
                PoolTimes = await GetPoolTimes();
                PoolClient.Listen();
                _poolTimer.Start();
            });

            PoolPower = -1;
        }

        /// <summary>
        /// Serializes the time intervals and saves the data int a file.
        /// </summary>
        /// <param name="obj"></param>
        public static async void Serialize(object obj)
        {
            ConvertIntervals();
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile file = await storageFolder.CreateFileAsync("test.txt", Windows.Storage.CreationCollisionOption.ReplaceExisting);

            var serializer = new DataContractJsonSerializer(typeof(List<TimeSelectorCharacteristic>));

            using (var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
            {
                using (var outputStream = stream.GetOutputStreamAt(0))
                {
                    serializer.WriteObject(outputStream.AsStreamForWrite(), PoolTimes);
                }
            }
        }

        /// <summary>
        /// Sorts intervals.
        /// </summary>
        /// <returns>Sorted list.</returns>
        private static List<TimeSelectorCharacteristic> SortIntervals()
        {
            return PoolTimes.OrderBy(o => o.FromTime).ToList();
        }

        /// <summary>
        /// Merges overlapping intervals.
        /// </summary>
        private static void ConvertIntervals()
        {
            
            var oldArray = SortIntervals();
            List<TimeSelectorCharacteristic> newArray = new List<TimeSelectorCharacteristic>();
            if (oldArray.Count == 0)
            {
                return;
            }

            newArray.Add(oldArray[0]);
            for (int i = 1; i < PoolTimes.Count; i++)
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
            }
            newArray.Reverse();
            PoolTimes = newArray;
        }

        /// <summary>
        /// Deserializes timings.
        /// </summary>
        /// <returns>List of timings.</returns>
        public static async Task<List<TimeSelectorCharacteristic>> GetPoolTimes()
        {
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var f = await storageFolder.TryGetItemAsync("test.txt");
            if (f == null)
            {
                return new List<TimeSelectorCharacteristic>();
            }
            var file = await storageFolder.OpenStreamForReadAsync("test.txt");

            var serializer = new DataContractJsonSerializer(typeof(List<TimeSelectorCharacteristic>));
            return (List<TimeSelectorCharacteristic>)serializer.ReadObject(file);
        }


        private async static void OnESPConnected()
        {
            _isESPConnected = true;
            PoolPower = await PoolClient.GetPoolStatus();
        }

        private static void OnESPDisconnected()
        {
            _isESPConnected = false;
        }

        /// <summary>
        /// Checks whether it is time to turn on or off the pool according to the times.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async static void CheckPoolTime(object sender, ElapsedEventArgs e)
        {
            try
            {
                PoolPower = await PoolClient.GetPoolStatus();
            }
            catch
            {
                return;
            }
            _poolTimer.Stop();
            var hour = DateTime.Now.Hour - 1;
            var half = (PoolTimes.Count - 1) / 2;
            int lIndex = 0;
            int HIndex = PoolTimes.Count - 1;
            int hourFromSelector;
            int numberIndex = -1;

            while (lIndex <= HIndex && numberIndex == -1)
            {
                hourFromSelector = PoolTimes[half].FromTime;

                if (hourFromSelector == hour)
                {
                    if (PoolPower == 0)
                    {
                        await SetESPStatus(true);
                    }
                    _poolTimer.Start();
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

            }

            if (HIndex != PoolTimes.Count - 1)
            {
                if (PoolPower == 0)
                {
                    if ((PoolTimes[lIndex].FromTime < hour &&
                        PoolTimes[lIndex].ToTime > hour) ||
                        (PoolTimes[HIndex].FromTime < hour &&
                        PoolTimes[HIndex].ToTime > hour))
                    {
                        SetESPStatus(true);
                    }
                }
                else if (PoolPower == 1)
                {
                    if (!((PoolTimes[lIndex].FromTime < hour &&
                        PoolTimes[lIndex].ToTime > hour) ||
                        (PoolTimes[HIndex].FromTime < hour &&
                        PoolTimes[HIndex].ToTime > hour)))
                    {
                        SetESPStatus(false);
                    }
                }
            }
            else
            {
                if (PoolPower == 0)
                {
                    if (PoolTimes[HIndex].FromTime <= hour && PoolTimes[HIndex].ToTime > hour)
                    {
                        SetESPStatus(true);
                    }
                }
                else if (PoolPower == 1)
                {
                    if (PoolTimes[HIndex].FromTime > hour || PoolTimes[HIndex].ToTime <= hour)
                    {
                        SetESPStatus(false);
                    }
                }
            }
            _poolTimer.Start();
        }

        public static void SetManualMode(bool manualMode)
        {
            switch (manualMode)
            {
                case true:
                    _poolTimer.Stop();
                    break;
                case false:
                    _poolTimer.Start();
                    break;
            }
        }

        /// <summary>
        /// Changes status of pool.
        /// </summary>
        /// <param name="obj">If true - turns pool on. If false - tunrs it off.</param>
        /// <returns></returns>
        public static async Task SetESPStatus(object obj)
        {
            bool newStatus;
            if (obj != null && obj is bool)
            {
                newStatus = (bool)obj;

                if (PoolPower == 0 && newStatus == true)
                {
                    try
                    {
                        await Task.Run(new Action(PoolClient.TurnOn));
                    }
                    catch { }
                }
                else if (PoolPower == 1 && newStatus == false)
                {
                    try
                    {
                        await Task.Run(new Action(PoolClient.TurnOff));
                    }
                    catch { }
                }
            }
            else if (PoolPower == 1)
            {
                try
                {
                    await Task.Run(new Action(PoolClient.TurnOff));
                }
                catch
                { }
            }
            else if (PoolPower == 0)
            {
                try
                {
                    await Task.Run(new Action(PoolClient.TurnOn));
                }
                catch
                { }
            }
            await UpdatePoolPower();
            OnPoolStateChanged?.Invoke();
        }

        private static async Task UpdatePoolPower()
        {
            int power = -1;
            try
            {
                power = await Task.Run(PoolClient.GetPoolStatus);
            }
            catch { }
            finally
            {
                PoolPower = power;
            }
        }
    }
}
