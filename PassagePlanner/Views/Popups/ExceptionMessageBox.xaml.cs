using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.ComponentModel;
using System.Drawing;
using System.Reflection;

namespace PassagePlanner
{
    /// <summary>
    /// Interaction logic for ExceptionMessageBox.xaml
    /// </summary>
    public partial class ExceptionMessageBox : MetroWindow
    {
        ButtonType _defaultButton = ButtonType.undefined;

        public ExceptionMessageBox(string message, string title, MessageBoxButton buttonSetup)
            : this(message, title, buttonSetup, string.Empty, string.Empty, string.Empty, ButtonType.undefined)
        {
        }

        public ExceptionMessageBox(string message, string title, MessageBoxButton buttonSetup, string okYesButtonName, string noButtonName, string cancelButtonName, ButtonType defaultButton)
        {
            InitializeComponent();

            About about = new About();
            string versionText = about.AssemblyProduct + " " + about.AssemblyVersion;

            this.textBlockMessage.Text = "Error in " + versionText + ":\n" + message;
            this.Title = title;
            
            switch (buttonSetup)
            { 
                case MessageBoxButton.OK:
                    buttonOkYes.Content = okYesButtonName == string.Empty ? "OK" : okYesButtonName;
                    buttonCancel.Focusable = false;
                    buttonNo.Visibility = Visibility.Collapsed;
                    buttonCancel.Visibility = Visibility.Collapsed;
                    buttonCancel.Focusable = false;
                    break;

                case MessageBoxButton.OKCancel:
                    buttonOkYes.Content = okYesButtonName == string.Empty ? "OK" : okYesButtonName;
                    buttonCancel.Content = cancelButtonName == string.Empty ? "Cancel" : cancelButtonName;
                    buttonNo.Visibility = Visibility.Collapsed;
                    buttonNo.Focusable = false;
                    break;

                case MessageBoxButton.YesNo:
                    buttonOkYes.Content = okYesButtonName == string.Empty ? "Yes" : okYesButtonName;
                    buttonNo.Content = noButtonName == string.Empty ? "No" : noButtonName;
                    buttonCancel.Visibility = Visibility.Collapsed;
                    buttonCancel.Focusable = false;
                    break;

                case MessageBoxButton.YesNoCancel:
                    buttonOkYes.Content = okYesButtonName == string.Empty ? "Yes" : okYesButtonName;
                    buttonNo.Content = noButtonName == string.Empty ? "No" : noButtonName;
                    buttonCancel.Content = cancelButtonName == string.Empty ? "Cancel" : cancelButtonName;
                    break;
            }

            _defaultButton = defaultButton;

            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

        }

        private void buttonOkYes_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();    
        }

        private void buttonNo_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = null;
            this.Close();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
           
        }

        private void MetroWindow_ContentRendered(object sender, EventArgs e)
        {

        }
    }
}
