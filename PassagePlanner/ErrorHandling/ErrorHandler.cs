using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassagePlanner
{
    public static class ErrorHandler
    {
        public static void Show(Exception ex)
        {
            var messageBox = new ExceptionMessageBox(ex.ToString(), "Error", System.Windows.MessageBoxButton.OK);
            messageBox.ShowDialog();
        }
    }
}
