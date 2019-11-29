using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace HomeAutomationUWP.Converters
{
    public class BoolToOnOFfOpposite : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool)
            {
                switch((bool)value)
                {
                    case true:
                        return "Off";
                        break;
                    case false:
                        return "On";
                        break;
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
