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
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace HomeAutomationUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PoolMenu : Page, INotifyPropertyChanged
    {
        public PoolMenu()
        {
            DataContext = new PoolMenuModel();
            this.InitializeComponent();
        }

        private int _fromTime = 10;

        public event PropertyChangedEventHandler PropertyChanged;

        public int FromTime
        {
            get
            {
                return _fromTime;
            }
            set
            {
                _fromTime = value;
                NotifyPropertyChanged("FromTime");
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
