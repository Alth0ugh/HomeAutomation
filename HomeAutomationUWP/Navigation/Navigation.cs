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

        /// <summary>
        /// Navigates to previous page.
        /// </summary>
        public static void GoBack()
        {
            if (Frame.CanGoBack)
            {
                Frame?.GoBack();
            }
        }

        /// <summary>
        /// Navigates to a given page.
        /// </summary>
        /// <param name="page">Page to navigate.</param>
        public static void Navigate(Type page)
        {
            Frame?.Navigate(page);
        }
    }
}
