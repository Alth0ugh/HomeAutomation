using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace HomeAutomationUWP.Controls
{
    public sealed partial class ColorTemperaturePicker : UserControl
    {
        public static readonly DependencyProperty TemperatureProperty = DependencyProperty.Register(nameof(Temperature), typeof(double), typeof(ColorTemperaturePicker), PropertyMetadata.Create((double)6000));

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public double Temperature
        {
            get
            {
                return (double)GetValue(TemplateProperty);
            }
            set
            {
                if (value >= 1700 && value <= 6500)
                {
                    SetValue(TemperatureProperty, value);
                }
            }
        }

        public ColorTemperaturePicker()
        {
            this.InitializeComponent();
            layout.DataContext = this;
        }
    }
}
