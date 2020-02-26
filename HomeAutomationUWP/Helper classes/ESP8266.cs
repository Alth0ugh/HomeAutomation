using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Windows.UI.Core;
using System.Timers;
using System.Diagnostics;

namespace HomeAutomationUWP.Helper_classes
{
    public class ESP8266
    {
        private bool _isListening = false;
        private bool _isESPConnected = false;

        public string IpAddress { get; set; }
        public int Port { get; set; }

        private TcpClient _client;
        private TcpListener _listener;
        private SslStream _sslStream;
        private X509Certificate2 _certificate;

        public delegate void OnConnectedHandler();
        public event OnConnectedHandler OnConnected;

        public delegate void OnDisconnectedHandler();
        public event OnDisconnectedHandler OnDisconnected;

        private Timer _reconnectTimer;

        public ESP8266()
        {
            _listener = new TcpListener(443);
            _reconnectTimer = new Timer(6000);
            _reconnectTimer.Elapsed += new ElapsedEventHandler(TestConnectivity);
            LoadCertificate();
        }

        /// <summary>
        /// Checks if ESP8266 is still connected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TestConnectivity(object sender, ElapsedEventArgs e)
        {
            var message = "areYouAlive";
            try
            {
                string response = string.Empty;
                Write(MakeMessage(message));
                do
                {
                    response += (char)ReadByte();
                } while (_client.Available > 0);
            }
            catch
            {
                if (_isESPConnected)
                {
                    OnDisconnected?.Invoke();
                    Task.Run(new Action(Listen));
                    _isESPConnected = false;
                }
            }
        }

        /// <summary>
        /// Loads server certificate.
        /// </summary>
        private void LoadCertificate()
        {
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            _certificate = new X509Certificate2(storageFolder.Path + @"\Server.p12");
        }

        public void Listen()
        {
            if (_isListening)
            {
                return;
            }
            _isListening = true;
            _listener.Start();
            _client = _listener.AcceptTcpClient();
            _sslStream = new SslStream(_client.GetStream(), false);
            try
            {
                _sslStream.AuthenticateAsServer(_certificate,
                    clientCertificateRequired: false,
                    enabledSslProtocols: System.Security.Authentication.SslProtocols.Tls11,
                    checkCertificateRevocation: false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _reconnectTimer.Start();
                _listener.Stop();
                _isListening = false;
            }
            _isESPConnected = true;
            OnConnected?.Invoke();
        }

        /// <summary>
        /// Creates message for sending.
        /// </summary>
        /// <param name="message">Message to convert.</param>
        /// <returns>Message ready to be sent in bytes.</returns>
        public byte[] MakeMessage(string message)
        {
            List<byte> array = new List<byte>(ASCIIEncoding.ASCII.GetBytes(message));
            array.Add((byte)'\n');
            return array.ToArray();
        }

        /// <summary>
        /// Writes message.
        /// </summary>
        /// <param name="message"></param>
        public void Write(byte[] message)
        {
            try
            {
                _sslStream.Write(message, 0, message.Length);
            }
            catch (Exception e)
            {
                OnDisconnected?.Invoke();
                throw e;
            }
        }

        /// <summary>
        /// Reads byte from buffer.
        /// </summary>
        /// <returns>The read byte.</returns>
        public int ReadByte()
        {
            int character;

            try
            {
                character = _sslStream.ReadByte();
            }
            catch (Exception e)
            {
                OnDisconnected?.Invoke();
                throw e;
            }

            return character;
        }
    }
}
