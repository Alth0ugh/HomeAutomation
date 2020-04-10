using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRServer.Interfaces
{
    public interface ISignalrServer
    {
        Task<int> GetPoolStatus();
    }
}
