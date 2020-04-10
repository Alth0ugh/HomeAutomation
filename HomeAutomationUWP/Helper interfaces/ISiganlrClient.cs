using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeAutomationUWP.Helper_interfaces
{
    public interface ISiganlrClient
    {
        Task UpdatePoolStatus(int newValue);
    }
}
