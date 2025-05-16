using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using System.Data;
using System.IO;
using System.Windows.Xps.Packaging;
using System.Xml;
using System.Globalization;
using System.Windows.Media.Animation;

namespace PassagePlanner
{
    /// <summary>
    /// Interaction logic for SquatAndUkcUC.xaml
    /// </summary>
    public partial class SquatAndUkcUC : UserControl
    {
        RouteViewModel _routeVM = null;
        FixedDocument _squatDoc = null;
        FixedDocument _ukcDoc = null;

        string _reportTemplatesDirectory = string.Empty;

        private const int DATAGRID_FONTSIZE = 10;

        public SquatAndUkcUC()
        {
            
            InitializeComponent();
            
            try
            {
                ViewModelLocator locator = new ViewModelLocator();
                _routeVM = locator.RouteVM;

                _reportTemplatesDirectory = FileManager.FindDirectory("ReportTemplates");
            }
            catch (Exception ex)
            {
                ErrorHandler.Show(ex);
            }
        }

        private void FixedDocument_Loaded(object sender, RoutedEventArgs e)
        {
            FixedDocument fixedDocument = sender as FixedDocument;
            fixedDocument.PrintTicket = PageOrientation.Landscape;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Copy some values from RouteViewModel to SquatAndUkcViewModel
                ViewModelLocator locator = new ViewModelLocator();
                RouteViewModel routeVM = locator.RouteVM;
                SquatViewModel squatVM = locator.SquatVM;
                VezzelViewModel vesselVM = locator.VesselVM;

                squatVM.UkcStatusText = string.Empty;

                if (squatVM.MinUkcRequired == 0)
                {
                    squatVM.MinUkcRequired = vesselVM.MinUkcRequired;
                }
                if (squatVM.VesselBeam == 0)
                {
                    squatVM.VesselBeam = vesselVM.VesselBeam;
                }
                if (squatVM.VesselDraughtFore == 0)
                {
                    squatVM.VesselDraughtFore = routeVM.DraughtDepartureFore;
                }
                if (squatVM.VesselDraughtAft == 0)
                {
                    squatVM.VesselDraughtAft = routeVM.DraughtDepartureAft;
                }
                if (squatVM.BlockCoefficient == 0)
                {
                    squatVM.BlockCoefficient = vesselVM.GetBlockCoefficientAtDraught(squatVM.VesselDraughtMid);
                }
                if ((squatVM.WaypointEta == null || squatVM.WaypointEta == DateTime.MinValue) && squatVM.SelectedWaypoint != null && squatVM.SelectedWaypoint.ETD != null && squatVM.SelectedWaypoint.ETD > DateTime.MinValue)
                {
                    squatVM.WaypointEta = (DateTime)squatVM.SelectedWaypoint.ETD;
                }

                if (squatVM.SelectedWaypoint != null)
                {
                    squatVM.WaterDepth = squatVM.SelectedWaypoint.MinDepth;
                    if (squatVM.SelectedWaypoint.LegSpeedSetting != null)
                    {
                        squatVM.Speed = (double)squatVM.SelectedWaypoint.LegSpeedSetting;
                    }
                }

                if ((TabItem)SquatMainTabControl.SelectedItem != null)
                {
                    if (((TabItem)SquatMainTabControl.SelectedItem).Name == "SquatCalculationsTab")
                    {
                        CreateSquatCalculationsPage();
                    }
                    else if (((TabItem)SquatMainTabControl.SelectedItem).Name == "UkcDeterminationTab")
                    {
                        CreateUkcDeterminationPage();
                    }
                }

            }
            catch (Exception ex)
            {
                ErrorHandler.Show(ex);
            }
        
        }

        private void CreateSquatCalculationsPage()
        {
            _squatDoc = new FixedDocument();
            StreamReader squatCalculationsReader = new StreamReader(new FileStream(_reportTemplatesDirectory + "ReportSquatCalculations.xaml", FileMode.Open, FileAccess.Read));
            XmlTextReader squatCalculationsXmlTextReader = new XmlTextReader(squatCalculationsReader);
            FixedPage squatCalculationsPage = (FixedPage)XamlReader.Load(squatCalculationsXmlTextReader);
            squatCalculationsReader.Close();
            PageContent squatCalculationsContent = new PageContent();
            ((IAddChild)squatCalculationsContent).AddChild(squatCalculationsPage);
            _squatDoc.Pages.Add(squatCalculationsContent);

            TextBlock textBlockDate = ((TextBlock)(squatCalculationsPage.FindName("textBlockDate")));
            textBlockDate.Text = System.DateTime.Now.Date.ToShortDateString();

            TextBlock pageNo = ((TextBlock)(squatCalculationsPage.FindName("textBlockPageNo")));
            pageNo.Text = "1 (1)";

            // Add Passage Planner version text
            TextBlock tb2 = (TextBlock)(squatCalculationsPage.FindName("assemblyVersionText"));
            tb2.Text = GetAssemblyVersionText();

            // Enable scrolling in docViewer when mouse over datagrids
            DataGrid variousDepths = ((DataGrid)(squatCalculationsPage.FindName("variousDepths")));
            variousDepths.PreviewMouseWheel += dataGrid_PreviewMouseWheel;
            DataGrid variousSpeeds = ((DataGrid)(squatCalculationsPage.FindName("variousSpeeds")));
            variousSpeeds.PreviewMouseWheel += dataGrid_PreviewMouseWheel;

            documentViewerSquat.Document = _squatDoc;
            documentViewerSquat.FitToWidth();
        }

        private void CreateUkcDeterminationPage()
        {
            _ukcDoc = new FixedDocument();
            StreamReader ukcDeterminationReader = new StreamReader(new FileStream(_reportTemplatesDirectory + "ReportUkcDetermination.xaml", FileMode.Open, FileAccess.Read));
            XmlTextReader ukcDeterminationXmlTextReader = new XmlTextReader(ukcDeterminationReader);
            FixedPage ukcDeterminationPage = (FixedPage)XamlReader.Load(ukcDeterminationXmlTextReader);
            ukcDeterminationReader.Close();
            PageContent ukcDeterminationContent = new PageContent();
            ((IAddChild)ukcDeterminationContent).AddChild(ukcDeterminationPage);
            _ukcDoc.Pages.Add(ukcDeterminationContent);

            TextBlock textBlockDate = ((TextBlock)(ukcDeterminationPage.FindName("textBlockDate")));
            textBlockDate.Text = System.DateTime.Now.Date.ToShortDateString();

            TextBlock pageNo = ((TextBlock)(ukcDeterminationPage.FindName("textBlockPageNo")));
            pageNo.Text = "1 (1)";

            // Add Passage Planner version text
            TextBlock tb2 = (TextBlock)(ukcDeterminationPage.FindName("assemblyVersionText"));
            tb2.Text = GetAssemblyVersionText();

            documentViewerUkc.Document = _ukcDoc;
            documentViewerUkc.FitToWidth();
        }


        private string GetAssemblyVersionText()
        {
            About about = new About();
            return "Printed from Seaware Passage Planner version " + about.AssemblyVersion + ". " + about.AssemblyCopyright + ". All Rights Reserved.";
        }

        /// <summary>
        /// Enable scrolling in docViewer when mouse is over data grid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Get the scroll viewer of documentViewer
            ScrollViewer dvScrollViewer = documentViewerSquat.Template.FindName("PART_ContentHost", documentViewerSquat) as ScrollViewer;
            dvScrollViewer.ScrollToVerticalOffset(dvScrollViewer.VerticalOffset - e.Delta);
            e.Handled = true;
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


        private void SquatMainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0].GetType() == typeof(TabItem))
            {
                TabItem selectedTab = e.AddedItems[0] as TabItem;  // Gets selected tab

                if (selectedTab.Name == "SquatCalculationsTab")
                {
                    CreateSquatCalculationsPage();
                }
                else if (selectedTab.Name == "UkcDeterminationTab")
                {
                    CreateUkcDeterminationPage();
                }
            }
        }

    }
}
