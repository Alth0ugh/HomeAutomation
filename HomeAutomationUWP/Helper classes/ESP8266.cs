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

namespace HomeAutomationUWP.Helper_classes
{
    class ESP8266
    {
        private const string _turnOn = "turnOn";
        private const string _turnOff = "turnOff";

        private bool _isListening = false;

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
            _reconnectTimer = new Timer(5000);
            _reconnectTimer.Stop();
            //_reconnectTimer.Elapsed += new ElapsedEventHandler(Handl)
            LoadCertificate();
        }

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
                _listener.Stop();
                _isListening = false;
            }
            OnConnected?.Invoke();
        }

        private byte[] MakeMessage(string message)
        {
            List<byte> array = new List<byte>(ASCIIEncoding.ASCII.GetBytes(message));
            array.Add((byte)'\n');
            return array.ToArray();
        }

        public void TurnOn()
        {
            try
            {
                Write(MakeMessage(_turnOn));
            }
            catch (Exception e)
            {
                Listen();
                throw e;
            }
        }

        public void TurnOff()
        {
            try
            {
                Write(MakeMessage(_turnOff));
            }
            catch (Exception e)
            {
                Listen();
                throw e;
            }
        }

        private void Write(byte[] message)
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

        private int ReadByte()
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

        public Task<int> GetPoolStatus()
        {
            string message = "getPoolStatus\n";
            try
            {
                Write(ASCIIEncoding.ASCII.GetBytes(message));
            }
            catch (Exception e)
            {
                throw e;
            }

            string response = string.Empty;
            char newCharacter;

            do
            {
                try
                {
                    newCharacter = (char)ReadByte();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                if (newCharacter != '\n')
                {
                    response += newCharacter;
                }
            } while (newCharacter != '\n');

            if (response == "true")
            {
                return Task.FromResult(1);
            }
            else
            {
                return Task.FromResult(0);
            }

        }
    }
}
