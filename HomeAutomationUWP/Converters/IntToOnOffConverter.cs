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
                        return "On";
                    case 0:
                        return "Off";
                    case -1:
                        return "Err";
                }
            }
            return "Err";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
