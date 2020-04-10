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
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Windows.Input;
using HomeAutomationUWP.Helper_classes;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace HomeAutomationUWP.Controls
{
    public sealed partial class TimeSelector : UserControl, INotifyPropertyChanged
    {
        public TimeSelector()
        {
            this.InitializeComponent();

            fromIncrease.Tag = new ButtonTag(ButtonTypes.FromTimeChange);
            fromDecrease.Tag = new ButtonTag(ButtonTypes.FromTimeChange);
            toIncrease.Tag = new ButtonTag(ButtonTypes.ToTimeChange);
            toDecrease.Tag = new ButtonTag(ButtonTypes.ToTimeChange);

            stackPanel.DataContext = this;
        }

        public static readonly DependencyProperty FromProperty = DependencyProperty.Register(nameof(From), typeof(ushort), typeof(TimeSelector), PropertyMetadata.Create((ushort)0));
        public static readonly DependencyProperty ToDependencyProperty = DependencyProperty.Register(nameof(To), typeof(ushort), typeof(TimeSelector), PropertyMetadata.Create((ushort)5));

        public TimeSelectorCharacteristic CurrentCharacteristic { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ushort From
        {
            get
            {
                return (ushort)GetValue(FromProperty);
            }
            set
            {
                if (value <= 24 && value >= 1 && value < To)
                {
                    SetValue(FromProperty, value);
                }
                else
                {
                    SetValue(FromProperty, (ushort)0);
                }
            }
        }
        public ushort To
        {
            get
            {
                return (ushort)GetValue(ToDependencyProperty);
            }
            set
            {
                if (value <= 24 && value >= 1 && value > From)
                {
                    SetValue(ToDependencyProperty, value);
                    ToText.Text = GetValue(ToDependencyProperty).ToString();
                }
                else
                {
                    SetValue(ToDependencyProperty, (ushort)1);
                    ToText.Text = GetValue(ToDependencyProperty).ToString();
                }
            }
        }

        public static readonly DependencyProperty DeleteProperty = DependencyProperty.Register(nameof(DeleteEntry), typeof(ICommand), typeof(TimeSelector), PropertyMetadata.Create(new RelayCommand(new Action<object>(o => { Debug.WriteLine("Delete"); }))));
        private ICommand _deleteProperty = new RelayCommand(new Action<object>(o => { Debug.WriteLine("Delete"); }));
        public ICommand DeleteEntry
        {
            get
            {
                return (ICommand)GetValue(DeleteProperty);
                //return _deleteProperty;
            }
            set
            {
                SetValue(DeleteProperty, value);
                //_deleteProperty = value;
                NotifyPropertyChanged("DeleteEntry");
            }
        }


        /// <summary>
        /// Changes value in textblocks.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                            From++;
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

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [DataContract]
    public class TimeSelectorCharacteristic : INotifyPropertyChanged
    {
        [DataMember]
        public ushort FromTime
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
        private ushort _fromTime = 0;
        [DataMember]
        public ushort ToTime
        {
            get
            {
                return _toTime;
            }
            set
            {
                _toTime = value;
                NotifyPropertyChanged("ToTime");
            }
        }
        private ushort _toTime = 1;

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
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
