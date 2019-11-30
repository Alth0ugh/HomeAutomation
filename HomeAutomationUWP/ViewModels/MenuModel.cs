using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HomeAutomationUWP.Helper_classes;
using HomeAutomationUWP.Views;

namespace HomeAutomationUWP.ViewModels
{
    public class MenuModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public MenuModel()
        {
            SetCommands();
        }

        private void SetCommands()
        {
            Pool = new RelayCommand(ShowPoolMenu);
        }

        private ICommand _pool;
        public ICommand Pool
        {
            get
            {
                return _pool;
            }
            set
            {
                _pool = value;
                NotifyPropertyChanged("Pool");
            }
        }

        private void ShowPoolMenu(object obj)
        {
            Navigation.Navigation.Navigate(typeof(PoolMenu));
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
