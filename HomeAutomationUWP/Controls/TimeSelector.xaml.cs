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
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace HomeAutomationUWP.Controls
{
    public sealed partial class TimeSelector : UserControl
    {
        public TimeSelector()
        {
            this.InitializeComponent();

            fromIncrease.Tag = new ButtonTag(ButtonTypes.FromTimeChange);
            fromDecrease.Tag = new ButtonTag(ButtonTypes.FromTimeChange);
            toIncrease.Tag = new ButtonTag(ButtonTypes.ToTimeChange);
            toDecrease.Tag = new ButtonTag(ButtonTypes.ToTimeChange);

            From = 0;
            To = 5;

            stackPanel.DataContext = this;
        }

        //public event PropertyChangedEventHandler PropertyChanged;
        //public static readonly DependencyProperty FromDependencyProperty = DependencyProperty.Register("From", typeof(ushort), typeof(TimeSelector), PropertyMetadata.Create(0));
        //public static readonly DependencyProperty ToDependencyProperty = DependencyProperty.Register("To", typeof(ushort), typeof(TimeSelector), PropertyMetadata.Create(0));

        private int _from = 0;
        private int _to = 1;

        public static readonly DependencyProperty FromProperty = DependencyProperty.Register(nameof(From), typeof(int), typeof(TimeSelector), PropertyMetadata.Create(10));
        public int From
        {
            get
            {
                return (int)GetValue(FromProperty);
                //return _from;
            }
            set
            {
                if (value <= 24 && value >= 1 && value < To)
                {
                    SetValue(FromProperty, value);
                    Debug.Write(value);
                    _from = value;
                    FromText.Text = _from.ToString();
                }
                else
                {
                    SetValue(FromProperty, 3);
                    _from = 0;
                    FromText.Text = _from.ToString();
                }
                //NotifyPropertyChanged("From");
            }
        }
        public int To
        {
            get
            {
                return _to;
                //return (ushort)GetValue(ToDependencyProperty);
            }
            set
            {
                if (value <= 24 && value >= 1 && value > From)
                {
                    _to = value;
                    ToText.Text = _to.ToString();
                  //  SetValue(ToDependencyProperty, value);
                }
                else
                {
                    _to = 5;
                    ToText.Text = _to.ToString();
                    //SetValue(ToDependencyProperty, (ushort)4);
                }
                //NotifyPropertyChanged("To");
            }
        }

        public void ChangeTime(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button != null)
            {
                string buttonContent;
                ButtonTypes buttonType;
                if (button.Content is string)
                {
                    buttonContent = (string)button.Content;
                }
                else
                {
                    return;
                }

                if (button.Tag is ButtonTag)
                {
                    buttonType = (button.Tag as ButtonTag).ButtonType;
                }
                else
                {
                    return;
                }

                switch (buttonType)
                {
                    case ButtonTypes.FromTimeChange:
                        if (buttonContent == "+")
                        {
                            //From++;
                            From = (From + 1);
                        }
                        else
                        {
                            From--;
                        }
                        break;
                    case ButtonTypes.ToTimeChange:
                        if (buttonContent == "+")
                        {
                            To++;
                        }
                        else
                        {
                            To--;
                        }
                        break;
                }
            }
        }
    }

    public class TimeSelectorCharacteristic
    {
        public int FromTime { get; set; }
        public int ToTime { get; set; }
        
    }

    public class ButtonTag
    {
        public ButtonTag(ButtonTypes buttonType)
        {
            ButtonType = buttonType;
        }
        public ButtonTypes ButtonType { get; set; }
    }

    public enum ButtonTypes
    {
        FromTimeChange,
        ToTimeChange
    }
}
