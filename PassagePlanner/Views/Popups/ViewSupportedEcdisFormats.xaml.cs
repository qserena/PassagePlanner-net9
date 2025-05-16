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
using EcdisLayer;

namespace PassagePlanner
{
    /// <summary>
    /// Interaction logic for ViewSupportedEcdisFormats.xaml
    /// </summary>
    public partial class ViewSupportedEcdisFormats : MetroWindow
    {

        public ViewSupportedEcdisFormats()
        {
            InitializeComponent();
            textBlockHeader.Text = "This system can open Ecdis files \nin the following formats:";
            showSupportedEcdisFormats();

            var about = new About();
            this.tbEcdisAssemblyVersion.Text = "EcdisPlugins.dll version " + about.EcdisPluginAssemblyVersion;

            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

        }

        private void showSupportedEcdisFormats()
        {
           // Get supported Ecdis plugins
           EcdisPluginHandler ecdisPluginHandler = new EcdisPluginHandler();
           List<string> supportedPlugins = ecdisPluginHandler.GetAvailablePlugins();

           string message = string.Empty;

            // The foreach loop below produces a string looking like:
            //
            //  Furuno  (*.txt)
            //  JRC  (*.rtn, *.rta)
            //  Kongsberg  (*.rut)
            //  Maris  (*.txt)
            //  SAM Electronics  (*.txt)
            //
            foreach (string pluginName in supportedPlugins)
            {
                IEcdisPlugin plugin = ecdisPluginHandler.GetPlugin(pluginName);
                List<string> supportedFileTypeList = plugin.GetSupportedFileTypes();
                string supportedFileTypes = "(";
                int i = 0;
                foreach (string fileType in supportedFileTypeList)
                {
                    if (i > 0)
                    {
                        supportedFileTypes += ", ";
                    }
                    supportedFileTypes += "*" + fileType;
                    i++;
                }
                supportedFileTypes += ")";

                message += pluginName + "  " + supportedFileTypes + "\n";
            }

           textBlockEcdisSystems.Text = message; 
        }

        private void buttonOkYes_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();    
        }
        
    }
}
