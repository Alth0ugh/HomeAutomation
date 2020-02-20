using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace HomeAutomationUWP.Helper_classes
{
    class ESP8266
    {
        public string IpAddress { get; set; }
        public int Port { get; set; }

        private TcpClient _client;
        private TcpListener _listener;
        private SslStream _sslStream;
        private X509Certificate2 _certificate;

        public ESP8266(string ip, int port)
        {
            IpAddress = ip;
            Port = port;

            _listener = new TcpListener(443);
            LoadCertificate();
            Task.Run(new Action(Listen));
        }

        private void LoadCertificate()
        {
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            _certificate = new X509Certificate2(storageFolder.Path + @"\Server.p12");
        }

        public void Listen()
        {
            _listener.Start();
            _client = _listener.AcceptTcpClient();
            _sslStream = new SslStream(_client.GetStream(), false);

            _sslStream.AuthenticateAsServer(_certificate, 
                clientCertificateRequired: false, 
                enabledSslProtocols: System.Security.Authentication.SslProtocols.Tls11, 
                checkCertificateRevocation: false);
        }
    }
}
