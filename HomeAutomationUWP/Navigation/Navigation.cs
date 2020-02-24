using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using HomeAutomationUWP.Helper_interfaces;
using HomeAutomationUWP.Helper_classes;

namespace HomeAutomationUWP.Navigation
{
    public static class Navigation
    {
        public static Frame Frame
        {
            get;
            set;
        }

        public static Button BackButton
        {
            get;
            set;
        }

        /// <summary>
        /// Navigates to previous page.
        /// </summary>
        public static void GoBack()
        {
            var page = (Frame.Content as Page);

            if (page != null)
            {
                var viewModel = page.DataContext;
                if (viewModel is INavigateBackAction)
                {
                    (viewModel as INavigateBackAction).OnNavigateBackAction(null);
                }
            }

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
            if (Frame == null)
            {
                return;
            }

            Frame.Navigate(page);

            var content = Frame.Content as Page;
            if (content is Page)
            {
                var viewModel = content.DataContext;
                if (viewModel is INavigateAction)
                {
                    Task.Run((viewModel as INavigateAction).NavigatedTo);
                }
            }
            //if (page is INavigateBackAction)
            //{
            //    var command = (BackButton?.Command as RelayCommand).Command;
            //    BackButton.Command = new RelayCommand(command, (page as INavigateBackAction).OnNavigateBackAction);
            //}
        }
    }
}
