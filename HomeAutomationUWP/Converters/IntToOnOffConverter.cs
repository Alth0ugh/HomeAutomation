using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace HomeAutomationUWP.Converters
{
    class IntToOnOffConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int)
            {
                switch ((int)value)
                {
                    case 1:
                        return true;
                    case 0:
                    case -1:
                        return false;
                }
            }
            return "Err";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is bool)
            {
                switch ((bool)value)
                {
                    case true:
                        return 1;
                    case false:
                        return 0;
                }
            }
            return 0;
        }
    }
}
