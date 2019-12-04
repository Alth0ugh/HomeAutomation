using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace HomeAutomationUWP.Helper_classes
{
    public class YeelightDevice
    {
        private static IPEndPoint _remoteEndPoint = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1982);
        private static IPEndPoint _localEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.116"), 1901);

        public async static void FindDevices()
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
                _foundDevices.Add(await _udpclient.ReceiveAsync());
            }
        }

        private YeelightDeviceCharacteristic GetDeviceCharacteristic(string messageBody)
        {
            var details = messageBody.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            var headers = messageBody.Split(new string[] { ": " }, 1, StringSplitOptions.None);

            Dictionary<string, string> data = new Dictionary<string, string>();

            for (int i = 0; i < 0; i = i + 2)
            {
                data.Add(headers[i], headers[i + 1]);
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
    }

    public class YeelightDeviceCharacteristic
    {
        public string IpAddresss { get; set; }
        public int Port { get; set; }
        public List<string> AvaliableMethods { get; set; }

        public YeelightDeviceCharacteristic(string ipAddress, int port, List<string> availableMethods)
        {
            IpAddresss = ipAddress;
            Port = port;
            AvaliableMethods = availableMethods;
        }
    }
}
