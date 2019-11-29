using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace HomeAutomationUWP.Converters
{
    class StringToUShort : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string)
            {
                var result = ushort.TryParse(value.ToString(), out ushort newValue);
                if (result)
                {
                    return newValue;
                }
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is ushort)
            {
                return value.ToString();
            }
            return "Err";
        }
    }
}
