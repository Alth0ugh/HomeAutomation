using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace HomeAutomationUWP.Navigation
{
    public static class Navigation
    {
        public static Frame Frame { get; set; }

        public static void GoBack()
        {
            if (Frame.CanGoBack)
            {
                Frame?.GoBack();
            }
        }

        public static void Navigate(Type page)
        {
            Frame?.Navigate(page);
        }
    }
}
