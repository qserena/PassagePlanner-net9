using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PassagePlanner
{
	/// <summary>
	/// Interaction logic for VesselUC.xaml
	/// </summary>
	public partial class VesselUC : UserControl
	{
		public VesselUC()
		{
			this.InitializeComponent();
            this.svScrollViewer.ScrollToVerticalOffset(170);
		}

        /// <summary>
        /// Selects the text in the textbox when textbox gets focus.
        /// So, when tabbing to the textbox you are ready to write the new text!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectText(object sender, RoutedEventArgs e)
        {
            TextBox tb = (sender as TextBox);

            if (tb != null)
            {
                tb.SelectAll();
            }
        }

        /// <summary>
        /// Selects text in textbox when clicking mouse in the textbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
        {
            TextBox tb = (sender as TextBox);

            if (tb != null)
            {
                if (!tb.IsKeyboardFocusWithin)
                {
                    e.Handled = true;
                    tb.Focus();
                }
            }
        }

	}
}