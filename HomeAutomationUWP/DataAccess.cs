using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using HomeAutomationUWP.Helper_interfaces;
using SignalRServer.Interfaces;

namespace HomeAutomationUWP
{
    public class DataAccess : ISignalrServer
    {
        private HubConnection _hubConnection;
        public async Task Init()
        {
            _hubConnection = new HubConnectionBuilder().WithUrl("http://192.168.1.116:5000/communicationHub").Build();
            await _hubConnection.StartAsync();
        }

        public void Bind<T>(string methodName, Func<T, Task> handler)
        {
            _hubConnection.On<T>(methodName, handler);
        }

        public async Task<int> GetPoolStatus()
        {
            return await _hubConnection.InvokeAsync<int>(nameof(ISignalrServer.GetPoolStatus));
        }
    }
}
