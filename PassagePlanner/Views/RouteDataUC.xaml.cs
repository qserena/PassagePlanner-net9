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
using C1.WPF.Maps;
using System.IO;
using ExcelLayer;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;

namespace PassagePlanner
{
    /// <summary>
    /// Description for RouteDataUC.
    /// </summary>
    public partial class RouteDataUC : UserControl
    {
        bool _isManualEditCommit = false;
        /// <summary>
        /// Initializes a new instance of the RouteDataUCView class.
        /// </summary>
        public RouteDataUC()
        {
            InitializeComponent();

            bool designMode = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
          
            if (!designMode)
            {
                if (dataGridWaypoints != null)
                {
                    this.dataGridWaypoints.Focus();
                }
            }
        }

        public ObservableCollection<Waypoint> Waypoints { get; set; }
        public Route SelectedRoute { get; set; }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModelLocator locator = new ViewModelLocator();
            RouteViewModel viewModel = locator.RouteVM;

            // data binding
            dataGridWaypoints.ItemsSource = viewModel.Waypoints;
                

            // Hack to force edit row to appear for empty collections
            if (viewModel.Waypoints.Count == 0)
            {
                viewModel.Waypoints.Add(new Waypoint());
                viewModel.Waypoints.RemoveAt(0);
            }

        }

        
        private void dataGridWaypoints_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModelLocator locator = new ViewModelLocator();
            RouteViewModel viewModel = locator.RouteVM;

            if (!viewModel.IgnoreSelectedItemsDuringOperation && dataGridWaypoints.SelectedItems != null)
            {
                viewModel.SelectedItemsInGrid.Clear();

                if (dataGridWaypoints.SelectedItems.Count > 0)
                {
                    foreach (var item in dataGridWaypoints.SelectedItems)
                    {
                        if (item.GetType() == typeof(Waypoint))
                        {
                            viewModel.SelectedItemsInGrid.Add((Waypoint)item);
                        }
                    }
                }
            }

        }

        // Forces update in view model after each cell edit (rather than when the entire row is edited) 
        private void dataGridWaypoints_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            isCellInEditionMode = false;

            if (!_isManualEditCommit)
            {
                _isManualEditCommit = true;
                DataGrid grid = (DataGrid)sender;
                grid.CommitEdit(DataGridEditingUnit.Row, true);

                ViewModelLocator locator = new ViewModelLocator();
                locator.RouteVM.UpdateWaypointRelatedStuff(e);
               

                _isManualEditCommit = false;
            }
        }

        /// <summary>
        /// Sets the same value for all waypoints for the column which header is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void columnHeader_Click(object sender, RoutedEventArgs e)
        {
            var columnHeader = sender as DataGridColumnHeader;
            if (columnHeader != null && columnHeader.Column != null)
            {
                PropertyPath path = null;
                string headerName = (string)columnHeader.Content;

                if (columnHeader.Column.GetType() == typeof(DataGridTextColumn))
                {
                    path = ((Binding)(((DataGridTextColumn)(columnHeader.Column)).Binding)).Path;
                }
                else if (columnHeader.Column.GetType() == typeof(DataGridComboBoxColumn))
                {
                    path = ((Binding)(((DataGridComboBoxColumn)(columnHeader.Column)).SelectedItemBinding)).Path;
                }

                if (path != null)
                {
                    ViewModelLocator locator = new ViewModelLocator();
                    locator.RouteVM.SetValueForAllWaypoints(headerName, path);
                }
            }
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

        /// <summary>
        /// This event handler is implemented for the following case:
        /// If SelectedDate is set to DateTime.MinValue and user opens the calendar,
        /// it is better that the calendar shows current date, than DateTime.MinValue.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DatePicker_CalendarOpened(object sender, RoutedEventArgs e)
        {
            DatePicker datePicker = (DatePicker)sender;

            if (datePicker.SelectedDate == null || datePicker.SelectedDate == DateTime.MinValue)
            {
                datePicker.SelectedDate = DateTime.Today;
            }
        }

        private void dataGridWaypoints_CurrentCellChanged(object sender, EventArgs e)
        {
            if (sender != null)
            {
                if (((DataGrid)sender).CurrentCell.Column != null && ((DataGrid)sender).CurrentCell.Column.GetType() == typeof(DataGridComboBoxColumn))
                {
                    // Do nothing. This if statement is here to prevent a bug where the Leg Type combo box 
                    // switched value from Rhumbline to GreatCircle when you traversed with the arrow key
                    // and came from the left to this Leg Type column. Now this does not happen.
                }
                else
                {
                    dataGridWaypoints.BeginEdit();
                }
            }
        }

        private bool isCellInEditionMode = false;

        private void dataGridWaypoints_BeginningEdit(object sender, DataGridBeginningEditEventArgs dataGridBeginningEditEventArgs)
        {
            isCellInEditionMode = true;
        }

        private void dataGridWaypoints_KeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.Key == Key.Up || keyEventArgs.Key == Key.Down || keyEventArgs.Key == Key.Left || keyEventArgs.Key == Key.Right)
            {
                if (!isCellInEditionMode)
                    return;

                dataGridWaypoints.CommitEdit();

                var key = keyEventArgs.Key; // Key to send
                var target = dataGridWaypoints; // Target element
                var routedEvent = Keyboard.KeyDownEvent; // Event to send

                target.RaiseEvent(
                    new KeyEventArgs(
                        Keyboard.PrimaryDevice,
                        PresentationSource.FromVisual(target),
                        0,
                        key) { RoutedEvent = routedEvent }
                    );
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            
            TextBox tb = sender as TextBox;

            if (tb != null)
            {
                Dispatcher.BeginInvoke(new SelectAllDelegate(SelectAll), tb);
            }
        }

        private delegate void SelectAllDelegate(TextBox tb);

        private void SelectAll(TextBox tb)
        {
            tb.SelectAll();
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsData("WaypointForClipboard") || Clipboard.ContainsData("ListOfWaypointForClipboard"))
            {
                menuItemInsertWaypointBefore.IsEnabled = true;
                menuItemInsertWaypointAfter.IsEnabled = true;
            }
            else
            {
                menuItemInsertWaypointBefore.IsEnabled = false;
                menuItemInsertWaypointAfter.IsEnabled = false;
            }
        }

    }
}