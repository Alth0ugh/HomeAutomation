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

        public MenuModel()
        {
            SetCommands();
        }

        private void SetCommands()
        {
            Pool = new RelayCommand(ShowPoolMenu);
            Light = new RelayCommand(ShowLightModule);
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

        private ICommand _light;
        public ICommand Light
        {
            get
            {
                return _light;
            }
            set
            {
                _light = value;
                NotifyPropertyChanged("Light");
            }
        }

        private void ShowPoolMenu(object obj)
        {/*
            Page page;
            if (_activePages.ContainsKey("poolMenu"))
            {
                page = _activePages["poolMenu"];
            }
            else
            {
                _activePages.Add("poolMenu", new PoolMenu());
                page = _activePages["poolMenu"];
            }*/
            Navigation.Navigation.Navigate(typeof(PoolMenu));
        }

        private void ShowLightModule(object obj)
        {
            Navigation.Navigation.Navigate(typeof(LightControl));
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
