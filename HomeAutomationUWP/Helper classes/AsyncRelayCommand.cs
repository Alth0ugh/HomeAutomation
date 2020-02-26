using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HomeAutomationUWP.Helper_classes
{
    class AsyncRelayCommand : ICommand
    {
        private Func<object, Task> _command;

        public AsyncRelayCommand(Func<object, Task> command)
        {
            _command = command;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public async void Execute(object parameter)
        {
            await _command(parameter);
        }
    }
}
