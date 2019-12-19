using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using Windows.UI.Xaml;
using Windows.Data.Json;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.IO;
using System.Diagnostics;

namespace HomeAutomationUWP.Helper_classes
{
    public class YeelightDevice
    {
        private static IPEndPoint _remoteEndPoint = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1982);
        private static IPEndPoint _localEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.116"), 1901);
        private TcpClient _tcpClient = new TcpClient();
        private Random _random = new Random();
        private DataContractJsonSerializer _serializer = new DataContractJsonSerializer(typeof(YeelightCommand));

        public YeelightDeviceCharacteristic DeviceCharacteristic { get; }

        public bool Connected
        {
            get
            {
                return _tcpClient.Connected;
            }
        }

        private YeelightDevice(YeelightDeviceCharacteristic deviceCharacteristic)
        {
            DeviceCharacteristic = deviceCharacteristic;
            _tcpClient.Client.Connect(deviceCharacteristic.IpAddress, deviceCharacteristic.Port);
        }

        /// <summary>
        /// Finds all Yeelight devices on network.
        /// </summary>
        /// <returns>List of device characteristics.</returns>
        public async static Task<List<YeelightDeviceCharacteristic>> FindDevices()
        {
            var _udpclient = new UdpClient();
            List<YeelightDeviceCharacteristic> _foundDevices = new List<YeelightDeviceCharacteristic>();
            // The following three lines allow multiple clients on the same PC
            _udpclient.ExclusiveAddressUse = false;
            _udpclient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _udpclient.ExclusiveAddressUse = false;
            // Bind, Join
            _udpclient.Client.Bind(_localEndPoint);
            _udpclient.JoinMulticastGroup(_remoteEndPoint.Address, _localEndPoint.Address);

            var bufferToSend = Encoding.UTF8.GetBytes("M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1982\r\nMAN: \"ssdp:discover\"\r\nST: wifi_bulb\r\n");
            await _udpclient.SendAsync(bufferToSend, bufferToSend.Length, _remoteEndPoint);

            while (_udpclient.Available > 0)
            {
                var buffer = (await _udpclient.ReceiveAsync()).Buffer;
                string message = string.Empty;
                foreach (var character in buffer)
                {
                    message += (char)character;
                }
                _foundDevices.Add(GetDeviceCharacteristic(message));
            }

            return _foundDevices;
        }

        /// <summary>
        /// Extracts all information needed to form YeelightDeviceCharacteristic.
        /// </summary>
        /// <param name="messageBody">Raw packet body.</param>
        /// <returns>Characteristic of the device.</returns>
        private static YeelightDeviceCharacteristic GetDeviceCharacteristic(string messageBody)
        {
            var details = messageBody.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            List<string> headers = new List<string>();
            List<string> information = new List<string>();

            for (int i = 2; i < details.Length; i++)                                //First two lines are unimportant
            {
                var line = details[i].Split(new char[] { ':' }, 2);
                if (line[0] != "support")
                {
                    line[1] = line[1].Trim();                                       //After ':' is whitespace
                }
                headers.Add(line[0]);
                information.Add(line[1]);
            }

            if (headers.Count != information.Count)                                 //Details must be in the same count as information
            {
                return null;
            }

            Dictionary<string, string> data = new Dictionary<string, string>();

            for (int i = 0; i < headers.Count; i++)
            {
                data.Add(headers[i], information[i]);
            }

            var ipAll = data["Location"].Replace("yeelight://", "").Split(':');
            var ip = ipAll[0];
            var parsing = int.TryParse(ipAll[1], out int result);

            if (!parsing)
            {
                return null;
            }

            List<string> availableMethods = new List<string>(data["support"].Split(' '));

            return new YeelightDeviceCharacteristic(ip, result, availableMethods);
        }

        public static Task<YeelightDevice> Connect(YeelightDeviceCharacteristic deviceCharacteristic)
        {

            return Task.FromResult(new YeelightDevice(deviceCharacteristic));
        }

        public bool SetBrightness(int value)
        {
            if (!DeviceCharacteristic.AvaliableMethods.Contains("set_bright"))
            {
                return false;
            }

            var random = new Random();
            var command = new YeelightCommand(random.Next(0, 100), "set_bright", value, "sudden", 0);
            var memoryStream = new MemoryStream();
            _serializer.WriteObject(memoryStream, command);
            memoryStream.Position = 0;
            var streamReader = new StreamReader(memoryStream);
            var json = streamReader.ReadLine() + "\r\n";
            var networkStream = new NetworkStream(_tcpClient.Client);
            string response = string.Empty;
            networkStream.Write(Encoding.ASCII.GetBytes(json), 0, json.Length);

            while (_tcpClient.Available > 0)
            {
                response += (char)networkStream.ReadByte();
            }
            Debug.WriteLine(response);
            return true;
        }

        public bool SetPower(bool value)
        {
            if (!DeviceCharacteristic.AvaliableMethods.Contains("set_power"))
            {
                return false;
            }

            var command = new YeelightCommand(_random.Next(1, 100), "set_power", "on", "smooth", 500);
            var networkStream = new NetworkStream(_tcpClient.Client);
            var ms = new MemoryStream();
            _serializer.WriteObject(ms, command);
            ms.Position = 0;
            var sr = new StreamReader(ms);
            var a = sr.ReadLine() + "\r\n";
            var ns = new NetworkStream(_tcpClient.Client);
            ns.Write(Encoding.ASCII.GetBytes(a), 0, a.Length);
            return true;
        }
    }

    [DataContract]
    public class YeelightCommand
    {
        [DataMember]
        public List<object> @params { get; set; }
        
        [DataMember]
        public string method { get; set; }

        [DataMember]
        public int id { get; set; }

        public YeelightCommand(int identificator, string methodName, params object[] parameters)
        {
            id = identificator;
            method = methodName;
            @params = new List<object>();
            foreach (var parameter in parameters)
            {
                @params.Add(parameter);
            }
        }
    }   


    public class YeelightDeviceCharacteristic : BindableBase
    {
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public List<string> AvaliableMethods { get; set; }
        private Visibility _connectButtonVisibility;
        public Visibility ConnectButtonVisibility
        {
            get
            {
                return _connectButtonVisibility;
            }
            set
            {
                _connectButtonVisibility = value;
                NotifyPropertyChanged("ConnectButtonVisibility");
            }
        }
        
        public YeelightDeviceCharacteristic(string ipAddress, int port, List<string> availableMethods)
        {
            ConnectButtonVisibility = Visibility.Collapsed;
            IpAddress = ipAddress;
            Port = port;
            AvaliableMethods = availableMethods;
        }
    }
}
