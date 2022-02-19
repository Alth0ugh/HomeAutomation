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
using Windows.UI.Xaml.Input;
using Newtonsoft.Json;
using HomeAutomationUWP.ViewModels;
using Windows.Networking.Sockets;
using System.Threading;

namespace HomeAutomationUWP.Helper_classes
{
    public class YeelightDevice
    {
        private static IPEndPoint _remoteEndPoint = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1982);
        private static IPEndPoint _localEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.117"), 1901);
        private TcpClient _tcpClient = new TcpClient();
        private Random _random = new Random();
        private DataContractJsonSerializer _serializer = new DataContractJsonSerializer(typeof(YeelightCommand));

        public YeelightDeviceCharacteristic DeviceCharacteristic { get; }

        public bool Connected
        {
            get
            {
                return _tcpClient.Client.Connected;
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
            var _udpClient = new UdpClient();
            List<YeelightDeviceCharacteristic> _foundDevices = new List<YeelightDeviceCharacteristic>();
            // The following three lines allow multiple clients on the same PC
            _udpClient.ExclusiveAddressUse = false;
            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _udpClient.ExclusiveAddressUse = false;
            // Bind, Join
            _udpClient.Client.Bind(_localEndPoint);
            _udpClient.JoinMulticastGroup(_remoteEndPoint.Address, _localEndPoint.Address);

            var bufferToSend = Encoding.UTF8.GetBytes("M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1982\r\nMAN: \"ssdp:discover\"\r\nST: wifi_bulb\r\n");
            await _udpClient.SendAsync(bufferToSend, bufferToSend.Length, _remoteEndPoint);

            Thread.Sleep(50);
            
            while (_udpClient.Available > 0)
            {
                var buffer = (await _udpClient.ReceiveAsync()).Buffer;
                string message = string.Empty;
                foreach (var character in buffer)
                {
                    message += (char)character;
                }
                _foundDevices.Add(GetDeviceCharacteristic(message));
            }

            _udpClient.Close();
            _udpClient.Dispose();
            /*
            List<YeelightDeviceCharacteristic> _foundDevices = new List<YeelightDeviceCharacteristic>();
            var socket = new DatagramSocket();
            await socket.BindEndpointAsync(new Windows.Networking.HostName("192.168.1.117"), "1901");
            socket.JoinMulticastGroup(new Windows.Networking.HostName("239.255.255.250"));

            socket.Conn*/
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

            parsing = int.TryParse(data["bright"], out int brightness);
            if (!parsing)
            {
                return null;
            }
            parsing = int.TryParse(data["ct"], out int colorTemperature);

            var deviceCharacteristic = new YeelightDeviceCharacteristic(ip, result, availableMethods) { Power = data["power"].Trim(), Brightness = brightness, ColorTemperature = colorTemperature};

            return deviceCharacteristic;
        }

        /// <summary>
        /// Connects to Yeelight device.
        /// </summary>
        /// <param name="deviceCharacteristic">Device characteristic of device to connect to.</param>
        /// <returns></returns>
        public static Task<YeelightDevice> Connect(YeelightDeviceCharacteristic deviceCharacteristic)
        {
            return Task.FromResult(new YeelightDevice(deviceCharacteristic));
        }
        
        /// <summary>
        /// Sets color temperature of device.
        /// </summary>
        /// <param name="value">Value from 2700 to 6500.</param>
        public void SetColorTemperature(int value)
        {
            if (!DeviceCharacteristic.AvaliableMethods.Contains("set_ct_abx"))
            {
                return;
            }
          
            DeviceCharacteristic.ColorTemperature = value;
            SendCommand(new YeelightCommand(_random.Next(1, 100), "set_ct_abx", value, "sudden", 0));
        }

        /// <summary>
        /// Sets brightness of a device.
        /// </summary>
        /// <param name="value">Value from 1 to 100.</param>
        public void SetBrightness(int value)
        {
            if (!DeviceCharacteristic.AvaliableMethods.Contains("set_bright"))
            {
                return;
            }

            DeviceCharacteristic.Brightness = value;
            SendCommand(new YeelightCommand(_random.Next(1, 100), "set_bright", value, "sudden", 0));
        }

        /// <summary>
        /// Sets power of a device.
        /// </summary>
        /// <param name="value">True to power on, false to power off.</param>
        public void SetPower(bool value)
        {
            if (!DeviceCharacteristic.AvaliableMethods.Contains("set_power"))
            {
                return;
            }

            if (value)
            {
                SendCommand(new YeelightCommand(_random.Next(1, 100), "set_power", "on", "smooth", 500));
            }
            else
            {
                SendCommand(new YeelightCommand(_random.Next(1, 100), "set_power", "off", "smooth", 500));
            }
        }

        /// <summary>
        /// Sets scene of the light.
        /// </summary>
        /// <param name="lightModes">The scene to use.</param>
        public void SetScene(LightModes lightModes)
        {
            switch (lightModes)
            {
                case LightModes.NightMode:
                    SendCommand(new YeelightCommand(_random.Next(1, 100), "set_scene", "nightlight", DeviceCharacteristic.Brightness));
                    break;
                case LightModes.DayMode:
                    SendCommand(new YeelightCommand(_random.Next(1, 100), "set_power", "on", "smooth", 0, 1));
                    break;
                default:
                    break;
            }
        }

        public void Disconnect()
        {
            _tcpClient.Close();
            _tcpClient.Dispose();
        }

        /// <summary>
        /// Sends command to a device.
        /// </summary>
        /// <param name="yeelightCommand">Instance of Yeelight command.</param>
        private void SendCommand(YeelightCommand yeelightCommand)
        {
            var networkStream = new NetworkStream(_tcpClient.Client);
            var memoryStream = new MemoryStream();
            _serializer.WriteObject(memoryStream, yeelightCommand);
            memoryStream.Position = 0;
            var streamReader = new StreamReader(memoryStream);
            var message = streamReader.ReadLine() + "\r\n";                     //Each message ends with \r\n
            try
            {
                networkStream.Write(MakeMessage(message), 0, message.Length);
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return;
            }
            finally
            {
                networkStream.Close();
                networkStream.Dispose();
                memoryStream.Close();
                memoryStream.Dispose();
            }
        }

        private byte[] MakeMessage(string text)
        {
            var message = text + "\r\n";
            return Encoding.ASCII.GetBytes(message);
        }

        private byte[] MakeMessage(string text)
        {
            var message = text + "\r\n";
            return ASCIIEncoding.ASCII.GetBytes(message);
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

    [JsonObject(MemberSerialization.OptIn)]
    public class YeelightDeviceCharacteristic : BindableBase
    {
        [JsonProperty]
        public string IpAddress { get; }
        [JsonProperty]
        public int Port { get; }
        public List<string> AvaliableMethods { get; }
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
        public string Power { get; set; }
        public int Brightness { get; set; }
        public int ColorTemperature { get; set; }

        public YeelightDeviceCharacteristic(string ipAddress, int port, List<string> availableMethods)
        {
            ConnectButtonVisibility = Visibility.Collapsed;
            IpAddress = ipAddress;
            Port = port;
            AvaliableMethods = availableMethods;
        }
    }
}
