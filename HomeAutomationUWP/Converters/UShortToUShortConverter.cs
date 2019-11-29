using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace HomeAutomationUWP.Converters
{
    class UShortToUShortConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ushort.Parse(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return ushort.Parse(value.ToString());
        }
    }
}
