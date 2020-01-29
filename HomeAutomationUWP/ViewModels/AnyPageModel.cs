using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HomeAutomationUWP.Navigation;
using HomeAutomationUWP.Helper_classes;

namespace HomeAutomationUWP.ViewModels
{
    public class AnyPageModel : BindableBase
    {
        private ICommand _backButton;
        public ICommand BackButton
        {
            get
            {
                return _backButton;
            }
            set
            {
                _backButton = value;
                NotifyPropertyChanged("BackButton");
            }
        }

        public AnyPageModel()
        {
            SetCommands();
        }

        private void SetCommands()
        {
            BackButton = new RelayCommand(Back);
        }

        private void Back(object obj)
        {
            Navigation.Navigation.GoBack();
        }
    }
}
