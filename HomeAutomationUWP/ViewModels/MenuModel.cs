using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HomeAutomationUWP.Helper_classes;
using HomeAutomationUWP.Views;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

namespace HomeAutomationUWP.ViewModels
{
    public class MenuModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Dictionary<string, Page> _activePages = new Dictionary<string, Page>();
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
            Page page;
            if (_activePages.ContainsKey("poolMenu"))
            {
                page = _activePages["poolMenu"];
            }
            else
            {
                _activePages.Add("poolMenu", new PoolMenu());
                page = _activePages["poolMenu"];
            }
            Navigation.Navigation.Navigate(page.GetType());
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
