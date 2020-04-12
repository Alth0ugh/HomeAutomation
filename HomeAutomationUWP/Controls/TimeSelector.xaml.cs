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
using MahApps.Metro.IconPacks;

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

        public static readonly DependencyProperty CurrentCharacteristicDependencyProperty = DependencyProperty.Register(nameof(CurrentCharacteristic), typeof(TimeSelectorCharacteristic), typeof(TimeSelector), PropertyMetadata.Create(new TimeSelectorCharacteristic() { FromTime = 0, ToTime = 1 }));
        public TimeSelectorCharacteristic CurrentCharacteristic
        {
            get
            {
                return (TimeSelectorCharacteristic)GetValue(CurrentCharacteristicDependencyProperty);
            }
            set
            {
                SetValue(CurrentCharacteristicDependencyProperty, value);
                NotifyPropertyChanged("CurrentCharacteristic");
            }
        }
       
        public event PropertyChangedEventHandler PropertyChanged;
        public static readonly DependencyProperty DeleteProperty = DependencyProperty.Register(nameof(DeleteEntry), typeof(ICommand), typeof(TimeSelector), PropertyMetadata.Create(new RelayCommand(new Action<object>(o => { }))));
        public ICommand DeleteEntry
        {
            get
            {
                return (ICommand)GetValue(DeleteProperty);
            }
            set
            {
                SetValue(DeleteProperty, value);
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
                PackIconMaterial buttonContent;
                ButtonTypes buttonType;
                if (button.Content is PackIconMaterial)
                {
                    buttonContent = (PackIconMaterial)button.Content;
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
                        if (buttonContent.Kind == PackIconMaterialKind.Plus)
                        {
                            if (CurrentCharacteristic.FromTime + 1 >= CurrentCharacteristic.ToTime)
                            {
                                CurrentCharacteristic.FromTime = 0;
                            }
                            else
                            {
                                CurrentCharacteristic.FromTime++;
                            }
                        }
                        else
                        {
                            if (CurrentCharacteristic.FromTime > 0)
                            {
                                CurrentCharacteristic.FromTime--;
                            }
                        }
                        break;
                    case ButtonTypes.ToTimeChange:
                        if (buttonContent.Kind == PackIconMaterialKind.Plus)
                        {
                            if (CurrentCharacteristic.ToTime < 24)
                            {
                                CurrentCharacteristic.ToTime++;
                            }
                        }
                        else
                        {
                            if (CurrentCharacteristic.ToTime - 1 > CurrentCharacteristic.FromTime)
                            {
                                CurrentCharacteristic.ToTime--;
                            }
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
