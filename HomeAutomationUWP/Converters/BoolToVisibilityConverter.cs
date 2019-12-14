using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace HomeAutomationUWP.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if(!(value is bool))
            {
                return Visibility.Collapsed; ;
            }

            if (!bool.Parse(value.ToString()))
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (!(value is Visibility))
            {
                return false;
            }

            if ((Visibility)value != Visibility.Visible)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
