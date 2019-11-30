using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using HomeAutomationUWP.ViewModels;
using HomeAutomationUWP.Navigation;
using System.Windows.Input;
using HomeAutomationUWP.Helper_classes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace HomeAutomationUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AnyPage : Page
    {
        public AnyPage()
        {
            this.InitializeComponent();
            Navigation.Navigation.Frame = frame;
            SetCommands();
            Navigation.Navigation.Navigate(typeof(Menu));
        }

        private void SetCommands()
        {
            Pool = new RelayCommand(ShowPool);
        }

        private void ShowPool(object obj)
        {
            Navigation.Navigation.Navigate(typeof(PoolMenu));
        }

        public ICommand Pool { get; set; }
    }
}
