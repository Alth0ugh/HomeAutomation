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

namespace HomeAutomationUWP.Controls
{
    public sealed partial class ColorTemperaturePicker : UserControl
    {
        public static readonly DependencyProperty TemperatureProperty = DependencyProperty.Register("Temperature", typeof(int), typeof(ColorTemperaturePicker), PropertyMetadata.Create(5000));
        public int Temperature
        {
            get
            {
                return (int)GetValue(TemplateProperty);
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
        }
    }
}
