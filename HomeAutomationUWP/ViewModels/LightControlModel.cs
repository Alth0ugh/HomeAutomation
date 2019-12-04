using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeAutomationUWP.Helper_classes;

namespace HomeAutomationUWP.ViewModels
{
    public class LightControlModel : BindableBase
    {
        private int _colorTemperature;
        public int ColorTemperature
        {
            get
            {
                return _colorTemperature;
            }
            set
            {
                _colorTemperature = value;
                NotifyPropertyChanged("ColorTemperature");
            }
        }


    }
}
