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
using System.Diagnostics;

namespace PassagePlanner
{
    /// <summary>
    /// Interaction logic for PassagePlan.xaml
    /// </summary>
    public partial class PassagePlanUC : UserControl
    {
        RouteViewModel _routeVM = null;
        FixedDocument _fixedDoc = null;
        int _noOfPages = 0;
        TextBlock _textBlockPageNo_GeneralNotesPart1 = null;
        TextBlock _textBlockPageNo_GeneralNotesPart2 = null;
        List<TextBlock> _textBlockPageNo_PartAPageN = null;
        List<TextBlock> _textBlockPageNo_PartBPageN = null;
        List<TextBlock> _textBlockPageNo_ChartsAndPublications = null;
        TextBlock _textBlockPageNo_AdditionalNotes = null;

        string _reportTemplatesDirectory = string.Empty;
        List<DataGrid> _partA_waypointGrids = null;
        List<DataGrid> _partB_waypointGrids = null;
        List<DataGrid> _chartsAndPublications_waypointGrids = null;

        PageContent _generalNotes1PageContent = new PageContent();
        PageContent _generalNotes2PageContent = new PageContent();

        List<FixedPage> _planAFixedPages = new List<FixedPage>();

        PageContent _additionalNotesPageContent = new PageContent();

        TextBox _textBoxGeneral = null;
        TextBox _textBoxNavigationalWarnings = null;
        TextBox _textBoxEmergencyAnchorages = null;
        TextBox _textBoxTnPNotices = null;
        TextBox _textBoxWeather = null;
        TextBox _textBoxRestrictedAreas = null;
        TextBox _textBoxRemarks = null;
        TextBox _textBoxAdditionalNotes = null;

        

        bool _isLoadedAtStartup = false; // To control that this user control is not loaded at startup (only when selecting this tab)

        private const int DATAGRID_FONTSIZE = 10;

        public PassagePlanUC()
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
        
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isLoadedAtStartup)
                {
                    UIServices.SetBusyState();

                    ViewModelLocator locator = new ViewModelLocator();
                    locator.RouteVM.IsLoading = true;

                    {
                        // Reset documentViewer.Document (remove previous FixedDocument)
                        documentViewer.Document = null;

                        _noOfPages = 0;
                        _textBlockPageNo_GeneralNotesPart1 = null;
                        _textBlockPageNo_GeneralNotesPart2 = null;
                        _textBlockPageNo_PartAPageN = null;
                        _textBlockPageNo_PartBPageN = null;
                        _textBlockPageNo_ChartsAndPublications = null;

                        // Create a new FixedDocument
                        _fixedDoc = new FixedDocument();

                        // Create and add "General Notes, part 1" to _fixedDoc
                        AddGeneralNotesPart1Page();

                        // Create and add "General Notes, part 2" to _fixedDoc
                        AddGeneralNotesPart2Page();

                        // Create and add "Passage Plan, part A" to _fixedDoc
                        AddPartAPages();

                        // Create and add "Passage Plan, part B" to _fixedDoc
                        AddPartBPages();

                        // Create and add "Charts and Publications" to _fixedDoc
                        AddChartsAndPublications();

                        // Create and add "Additional Notes" to _fixedDoc
                        AddAdditionalNotesPage();

                        // After all pages are created, we know how many pages there were
                        SetTotalNumberOfPages();

                        // set _fixedDoc as the document in documentViewer
                        documentViewer.Document = _fixedDoc;

                    }

                    locator.RouteVM.IsLoading = false;
                }

                _isLoadedAtStartup = true;

                documentViewer.FitToWidth();
               
            }
            catch (Exception ex)
            {
                ErrorHandler.Show(ex);
            }
        
        }

        private void AddGeneralNotesPart1Page()
        {
            StreamReader generalNotes1Reader = new StreamReader(new FileStream(_reportTemplatesDirectory + "PassagePlanGeneralNotes1.xaml", FileMode.Open, FileAccess.Read));
            XmlTextReader generalNotes1XmlTextReader = new XmlTextReader(generalNotes1Reader);
            FixedPage generalNotes1Page = (FixedPage)XamlReader.Load(generalNotes1XmlTextReader);
            
            generalNotes1Reader.Close();
            PageContent generalNotes1Content = new PageContent();
            ((IAddChild)generalNotes1Content).AddChild(generalNotes1Page);
            _fixedDoc.Pages.Add(generalNotes1Content);
            _noOfPages++;

            TextBlock textBlockDate = ((TextBlock)(generalNotes1Page.FindName("textBlockDate")));
            textBlockDate.Text = System.DateTime.Now.Date.ToShortDateString();
           

            _textBlockPageNo_GeneralNotesPart1 = ((TextBlock)(generalNotes1Page.FindName("textBlockPageNo")));
            _textBlockPageNo_GeneralNotesPart1.Text = _noOfPages.ToString();

            // Add Passage Planner version text
            TextBlock tb2 = (TextBlock)(generalNotes1Page.FindName("assemblyVersionText"));
            tb2.Text = GetAssemblyVersionText();

            _textBoxGeneral = ((TextBox)(generalNotes1Page.FindName("textBoxGeneral")));
            _textBoxGeneral.PreviewKeyDown += textBox_PreviewKeyDown_max7Lines;
        }

        private void AddGeneralNotesPart2Page()
        {
            StreamReader generalNotes2Reader = new StreamReader(new FileStream(_reportTemplatesDirectory + "PassagePlanGeneralNotes2.xaml", FileMode.Open, FileAccess.Read)); 
            XmlTextReader generalNotes2XmlTextReader = new XmlTextReader(generalNotes2Reader);
            FixedPage generalNotes2Page = (FixedPage)XamlReader.Load(generalNotes2XmlTextReader);
            generalNotes2Reader.Close();
            PageContent generalNotes2Content = new PageContent();
            ((IAddChild)generalNotes2Content).AddChild(generalNotes2Page);
            _fixedDoc.Pages.Add(generalNotes2Content);
            _noOfPages++;


            TextBlock textBlockDate = ((TextBlock)(generalNotes2Page.FindName("textBlockDate")));
            textBlockDate.Text = System.DateTime.Now.Date.ToShortDateString();

            _textBlockPageNo_GeneralNotesPart2 = ((TextBlock)(generalNotes2Page.FindName("textBlockPageNo")));
            _textBlockPageNo_GeneralNotesPart2.Text = _noOfPages.ToString();

            // Add Passage Planner version text
            TextBlock tb2 = (TextBlock)(generalNotes2Page.FindName("assemblyVersionText"));
            tb2.Text = GetAssemblyVersionText();

            _textBoxNavigationalWarnings = ((TextBox)(generalNotes2Page.FindName("textBoxNavigationalWarnings")));
            _textBoxNavigationalWarnings.PreviewKeyDown += textBox_PreviewKeyDown_max4Lines;

            _textBoxEmergencyAnchorages = ((TextBox)(generalNotes2Page.FindName("textBoxEmergencyAnchorages")));
            _textBoxEmergencyAnchorages.PreviewKeyDown += textBox_PreviewKeyDown_max4Lines;

            _textBoxTnPNotices = ((TextBox)(generalNotes2Page.FindName("textBoxTnPNotices")));
            _textBoxTnPNotices.PreviewKeyDown += textBox_PreviewKeyDown_max4Lines;

            _textBoxWeather = ((TextBox)(generalNotes2Page.FindName("textBoxWeather")));
            _textBoxWeather.PreviewKeyDown += textBox_PreviewKeyDown_max4Lines;

            _textBoxRestrictedAreas = ((TextBox)(generalNotes2Page.FindName("textBoxRestrictedAreas")));
            _textBoxRestrictedAreas.PreviewKeyDown += textBox_PreviewKeyDown_max4Lines;

            _textBoxRemarks = ((TextBox)(generalNotes2Page.FindName("textBoxRemarks")));
            _textBoxRemarks.PreviewKeyDown += textBox_PreviewKeyDown_max4Lines;
        }

        /// <summary>
        /// Create the pages in the Passage Plan printable report, called "Part A".
        /// One page if few waypoints. Otherwise more than one page.
        /// </summary>
        private void AddPartAPages()
        {
            // Calculate how many pages has to be created
            double numberOfPagesDouble = 1 + Convert.ToDouble(_routeVM.NoOfWaypoints - RouteViewModel.NUMBER_OF_WAYPOINTS_IN_REPORT_FIRST_PAGE) / Convert.ToDouble(RouteViewModel.NUMBER_OF_WAYPOINTS_IN_REPORT_FOLLOWING_PAGES);
            int numberOfPagePages = Convert.ToInt32(Math.Ceiling(numberOfPagesDouble));
        
            // Create a list with datagrids, one for each page
            _partA_waypointGrids = new List<DataGrid>();

            // Create a list with text boxes, holding page numbers
            _textBlockPageNo_PartAPageN = new List<TextBlock>();

            _routeVM.UpdateWaypointRelatedStuff(null);

            for (int i = 0; i < numberOfPagePages; i++)
            {
                // Read xaml file template
                StreamReader partAPageNReader = new StreamReader(new FileStream(_reportTemplatesDirectory + "PassagePlanPartA.xaml", FileMode.Open, FileAccess.Read));
                XmlTextReader partAPageNXmlTextReader = new XmlTextReader(partAPageNReader);
                FixedPage passagePlanAPageN = (FixedPage)XamlReader.Load(partAPageNXmlTextReader);
                partAPageNReader.Close();

                DataGrid pageN_waypointGrid = new DataGrid();
                //pageN_waypointGrid.Padding = new Thickness(0, 0, 0, -10);
                pageN_waypointGrid.BorderThickness = new Thickness(1, 1, 0, 0);
                pageN_waypointGrid.CanUserAddRows = false;
                pageN_waypointGrid.CanUserDeleteRows = false;
                pageN_waypointGrid.CanUserReorderColumns = false;
                pageN_waypointGrid.CanUserResizeColumns = false;
                pageN_waypointGrid.CanUserResizeRows = false;
                pageN_waypointGrid.CanUserSortColumns = false;
                pageN_waypointGrid.AutoGenerateColumns = false;
                pageN_waypointGrid.HeadersVisibility = DataGridHeadersVisibility.None;
                pageN_waypointGrid.PreviewMouseWheel += waypointGrid_PreviewMouseWheel;
                pageN_waypointGrid.FontSize = DATAGRID_FONTSIZE;
                pageN_waypointGrid.BorderBrush = Brushes.Black;

                // Add this datagrid to the list of datagrids (we put them into a list because we
                // create the datagrids and the report pages dynamically, and we have to databind
                // the datagrids to the pages, and thus we have to have some references to the datagrids)
                _partA_waypointGrids.Add(pageN_waypointGrid);

                if (_routeVM.ReportPages_Waypoints != null && _routeVM.ReportPages_Waypoints.Count > i)
                {
                    // Databind this datagrid to the proper page in the printable report
                    _partA_waypointGrids[i].ItemsSource = _routeVM.ReportPages_Waypoints[i];
                }

                

                DataGridTextColumn pageN_columnWpNo = new DataGridTextColumn();
                pageN_columnWpNo.Header = "WP No";
                pageN_columnWpNo.Width = 26;
                Binding pageN_columnWpNoBinding = new Binding("WaypointNo");
                pageN_columnWpNoBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnWpNoBinding.Mode = BindingMode.OneWay;
                pageN_columnWpNo.Binding = pageN_columnWpNoBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnWpNo);

                DataGridTextColumn pageN_columnWpName = new DataGridTextColumn();
                pageN_columnWpName.Header = "Waypoint Name";
                pageN_columnWpName.Width = 117;
                Binding pageN_columnWpNameBinding = new Binding("WaypointName");
                pageN_columnWpNameBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnWpNameBinding.Mode = BindingMode.OneWay;
                pageN_columnWpName.Binding = pageN_columnWpNameBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnWpName);

                DataGridTextColumn pageN_columnLatitude = new DataGridTextColumn();
                pageN_columnLatitude.Header = "Latitude";
                pageN_columnLatitude.Width = 69;
                Binding pageN_columnLatitudeBinding = new Binding("Latitude");
                pageN_columnLatitudeBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnLatitudeBinding.Mode = BindingMode.OneWay;
                pageN_columnLatitudeBinding.Converter = new PositionValueConverter();
                pageN_columnLatitudeBinding.ConverterParameter = "Latitude";
                pageN_columnLatitude.Binding = pageN_columnLatitudeBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnLatitude);

                DataGridTextColumn pageN_columnLongitude = new DataGridTextColumn();
                pageN_columnLongitude.Header = "Longitude";
                pageN_columnLongitude.Width = 73;
                Binding pageN_columnLongitudeBinding = new Binding("Longitude");
                pageN_columnLongitudeBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnLongitudeBinding.Mode = BindingMode.OneWay;
                pageN_columnLongitudeBinding.Converter = new PositionValueConverter();
                pageN_columnLongitudeBinding.ConverterParameter = "Longitude";
                pageN_columnLongitude.Binding = pageN_columnLongitudeBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnLongitude);

                DataGridTextColumn pageN_columnCourse = new DataGridTextColumn();
                pageN_columnCourse.Header = "CourseToNextWaypoint";
                pageN_columnCourse.Width = 40;
                Binding pageN_columnCourseBinding = new Binding("CourseToNextWaypoint");
                pageN_columnCourseBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnCourseBinding.Mode = BindingMode.OneWay;
                pageN_columnCourseBinding.StringFormat = "D3";
                pageN_columnCourse.Binding = pageN_columnCourseBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnCourse);

                DataGridTextColumn pageN_columnDistance = new DataGridTextColumn();
                pageN_columnDistance.Header = "DistanceToNextWaypoint";
                pageN_columnDistance.Width = 44;
                Binding pageN_columnDistanceBinding = new Binding("DistanceToNextWaypoint");
                pageN_columnDistanceBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnDistanceBinding.Mode = BindingMode.OneWay;
                pageN_columnDistanceBinding.StringFormat = "F1";
                pageN_columnDistance.Binding = pageN_columnDistanceBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnDistance);

                DataGridTextColumn pageN_columnLegSpeed = new DataGridTextColumn();
                pageN_columnLegSpeed.Header = "Speed";
                pageN_columnLegSpeed.Width = 50;
                Binding pageN_columnLegSpeedBinding = new Binding("LegSpeedSetting");
                pageN_columnLegSpeedBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnLegSpeedBinding.Mode = BindingMode.OneWay;
                pageN_columnLegSpeedBinding.StringFormat = "F1";
                pageN_columnLegSpeed.Binding = pageN_columnLegSpeedBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnLegSpeed);

                DataGridTextColumn pageN_columnWaterDepth = new DataGridTextColumn();
                pageN_columnWaterDepth.Header = "WaterDepth";
                pageN_columnWaterDepth.Width = 50;
                Binding pageN_columnWaterDepthBinding = new Binding("MinDepth");
                pageN_columnWaterDepthBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnWaterDepthBinding.Mode = BindingMode.OneWay;
                pageN_columnWaterDepthBinding.StringFormat = "F1";
                pageN_columnWaterDepth.Binding = pageN_columnWaterDepthBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnWaterDepth);

                DataGridTextColumn pageN_columnSquat = new DataGridTextColumn();
                pageN_columnSquat.Header = "Squat";
                pageN_columnSquat.Width = 35;
                Binding pageN_columnSquatBinding = new Binding("SquatUkc.SquatOpenWater");
                pageN_columnSquatBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnSquatBinding.Mode = BindingMode.OneWay;
                pageN_columnSquatBinding.StringFormat = "F1";
                pageN_columnSquat.Binding = pageN_columnSquatBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnSquat);

                DataGridTextColumn pageN_columnUkc = new DataGridTextColumn();
                pageN_columnUkc.Header = "Ukc";
                pageN_columnUkc.Width = 35;
                Binding pageN_columnUkcBinding = new Binding("SquatUkc.UkcOpenWater");
                pageN_columnUkcBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnUkcBinding.Mode = BindingMode.OneWay;
                pageN_columnUkcBinding.StringFormat = "F1";
                pageN_columnUkc.Binding = pageN_columnUkcBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnUkc);

                DataGridTextColumn pageN_columnEngineStatus = new DataGridTextColumn();
                pageN_columnEngineStatus.Header = "EngineStatus";
                pageN_columnEngineStatus.Width = 40;
                Binding pageN_columnEngineStatusBinding = new Binding("EngineStatus");
                pageN_columnEngineStatusBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnEngineStatusBinding.Mode = BindingMode.OneWay;
                pageN_columnEngineStatus.Binding = pageN_columnEngineStatusBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnEngineStatus);

                DataGridTextColumn pageN_columnNavWatchLevel = new DataGridTextColumn();
                pageN_columnNavWatchLevel.Header = "NavWatchLevel";
                pageN_columnNavWatchLevel.Width = 35;
                Binding pageN_columnNavWatchLevelBinding = new Binding("NavWatchLevel");
                pageN_columnNavWatchLevelBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnNavWatchLevelBinding.Mode = BindingMode.OneWay;
                pageN_columnNavWatchLevel.Binding = pageN_columnNavWatchLevelBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnNavWatchLevel);

                DataGridTextColumn pageN_columnSecurityLevel = new DataGridTextColumn();
                pageN_columnSecurityLevel.Header = "SecurityLevel";
                pageN_columnSecurityLevel.Width = 42;
                Binding pageN_columnSecurityLevelBinding = new Binding("SecurityLevel");
                pageN_columnSecurityLevelBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnSecurityLevelBinding.Mode = BindingMode.OneWay;
                pageN_columnSecurityLevel.Binding = pageN_columnSecurityLevelBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnSecurityLevel);

                DataGridTextColumn pageN_columnLegSailingTime = new DataGridTextColumn();
                pageN_columnLegSailingTime.Header = "TimeToNextWaypoint";
                pageN_columnLegSailingTime.Width = 54;
                Binding pageN_columnLegSailingTimeBinding = new Binding("TimeToNextWaypointAsString");
                pageN_columnLegSailingTimeBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnLegSailingTimeBinding.Mode = BindingMode.OneWay;
                //pageN_columnLegSailingTimeBinding.StringFormat = "F1";
                pageN_columnLegSailingTime.Binding = pageN_columnLegSailingTimeBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnLegSailingTime);

                DataGridTextColumn pageN_columnEtaWp = new DataGridTextColumn();
                pageN_columnEtaWp.Header = "ETA Waypoint";
                pageN_columnEtaWp.Width = 102;
                Binding pageN_columnEtaWpBinding = new Binding("ETD");
                pageN_columnEtaWpBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnEtaWpBinding.Mode = BindingMode.OneWay;
                pageN_columnEtaWpBinding.StringFormat = "g";
                pageN_columnEtaWpBinding.ConverterCulture = CultureInfo.CurrentCulture;
                pageN_columnEtaWp.Binding = pageN_columnEtaWpBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnEtaWp);

                DataGridTextColumn pageN_columnDistanceSailed = new DataGridTextColumn();
                pageN_columnDistanceSailed.Header = "DistanceSailed";
                pageN_columnDistanceSailed.Width = 44;
                Binding pageN_columnDistanceSailedBinding = new Binding("DistanceSailed");
                pageN_columnDistanceSailedBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnDistanceSailedBinding.Mode = BindingMode.OneWay;
                pageN_columnDistanceSailedBinding.StringFormat = "F1";
                pageN_columnDistanceSailed.Binding = pageN_columnDistanceSailedBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnDistanceSailed);

                DataGridTextColumn pageN_columnDistanceToGo = new DataGridTextColumn();
                pageN_columnDistanceToGo.Header = "DistanceToGoToDestination";
                pageN_columnDistanceToGo.Width = 44;
                Binding pageN_columnDistanceToGoBinding = new Binding("DistanceToGoToDestination");
                pageN_columnDistanceToGoBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnDistanceToGoBinding.Mode = BindingMode.OneWay;
                pageN_columnDistanceToGoBinding.StringFormat = "F1";
                pageN_columnDistanceToGo.Binding = pageN_columnDistanceToGoBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnDistanceToGo);

                DataGridTextColumn pageN_columnTimeToGoToDestination = new DataGridTextColumn();
                pageN_columnTimeToGoToDestination.Header = "TimeToGoToDestination";
                pageN_columnTimeToGoToDestination.Width = 54;
                Binding pageN_columnTimeToGoToDestinationBinding = new Binding("TimeToGoToDestinationAsString");
                pageN_columnTimeToGoToDestinationBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnTimeToGoToDestinationBinding.Mode = BindingMode.OneWay;
                pageN_columnTimeToGoToDestination.Binding = pageN_columnTimeToGoToDestinationBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnTimeToGoToDestination);

                DataGridTextColumn pageN_columnMaxIntervalsBetweenPosFix = new DataGridTextColumn();
                pageN_columnMaxIntervalsBetweenPosFix.Header = "MaxIntervalsBetweenPosFix";
                pageN_columnMaxIntervalsBetweenPosFix.Width = new DataGridLength(1.0, DataGridLengthUnitType.Star);
                Binding pageN_columnMaxIntervalsBetweenPosFixBinding = new Binding("MaxIntervalsPosFix");
                pageN_columnMaxIntervalsBetweenPosFixBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnMaxIntervalsBetweenPosFixBinding.Mode = BindingMode.OneWay;
                pageN_columnMaxIntervalsBetweenPosFix.Binding = pageN_columnMaxIntervalsBetweenPosFixBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnMaxIntervalsBetweenPosFix);

                // Get the panel in which the datagrid will reside
                StackPanel waypointPanelPageN = ((StackPanel)(passagePlanAPageN.FindName("waypointPanel")));

                // Add the datagrid to the panel
                waypointPanelPageN.Children.Add(pageN_waypointGrid);

                if (i > 0)
                {
                    // Hide/Collapse the upper base information grid, which is only meant for the first page
                    Border baseInformationSection = ((Border)(passagePlanAPageN.FindName("baseInformationSection")));
                    baseInformationSection.Visibility = System.Windows.Visibility.Collapsed;
                }

                // Add the page to the fixed document
                PageContent planAPageNContent = new PageContent();
                ((IAddChild)planAPageNContent).AddChild(passagePlanAPageN);
                _fixedDoc.Pages.Add(planAPageNContent);
                _noOfPages++;

                // Add current date
                TextBlock pageN_textBlockDate = ((TextBlock)(passagePlanAPageN.FindName("textBlockDate")));
                pageN_textBlockDate.Text = System.DateTime.Now.Date.ToShortDateString();

                // Add page number
                TextBlock tb = (TextBlock)(passagePlanAPageN.FindName("textBlockPageNo"));
                tb.Text = _noOfPages.ToString();
                _textBlockPageNo_PartAPageN.Add(tb);

                // Add Passage Planner version text
                TextBlock tb2 = (TextBlock)(passagePlanAPageN.FindName("assemblyVersionText"));
                tb2.Text = GetAssemblyVersionText();
            }

        }

        /// <summary>
        /// Create the pages in the Passage Plan printable report, called "Part B".
        /// One page if few waypoints. Otherwise more than one page.
        /// </summary>
        private void AddPartBPages()
        {
            // Calculate how many pages has to be created
            double numberOfPagesDouble = 1 + Convert.ToDouble(_routeVM.NoOfWaypoints - RouteViewModel.NUMBER_OF_WAYPOINTS_IN_REPORT_FIRST_PAGE) / Convert.ToDouble(RouteViewModel.NUMBER_OF_WAYPOINTS_IN_REPORT_FOLLOWING_PAGES);
            int numberOfPagePages = Convert.ToInt32(Math.Ceiling(numberOfPagesDouble));

            // Create a list with datagrids, one for each page
            _partB_waypointGrids = new List<DataGrid>();

            // Create a list with text boxes, holding page numbers
            _textBlockPageNo_PartBPageN = new List<TextBlock>();

            for (int i = 0; i < numberOfPagePages; i++)
            {
                // Read xaml file template
                StreamReader partBPageNReader = new StreamReader(new FileStream(_reportTemplatesDirectory + "PassagePlanPartB.xaml", FileMode.Open, FileAccess.Read));
                XmlTextReader partBPageNXmlTextReader = new XmlTextReader(partBPageNReader);
                FixedPage passagePlanBPageN = (FixedPage)XamlReader.Load(partBPageNXmlTextReader);
                partBPageNReader.Close();

                DataGrid pageN_waypointGrid = new DataGrid();

                pageN_waypointGrid.BorderThickness = new Thickness(1, 1, 0, 0);
                pageN_waypointGrid.CanUserAddRows = false;
                pageN_waypointGrid.CanUserDeleteRows = false;
                pageN_waypointGrid.CanUserReorderColumns = false;
                pageN_waypointGrid.CanUserResizeColumns = false;
                pageN_waypointGrid.CanUserResizeRows = false;
                pageN_waypointGrid.CanUserSortColumns = false;
                pageN_waypointGrid.AutoGenerateColumns = false;
                pageN_waypointGrid.HeadersVisibility = DataGridHeadersVisibility.None;
                pageN_waypointGrid.PreviewMouseWheel += waypointGrid_PreviewMouseWheel;
                pageN_waypointGrid.FontSize = DATAGRID_FONTSIZE;
                pageN_waypointGrid.BorderBrush = Brushes.Black;

                // Add this datagrid to the list of datagrids (we put them into a list because we
                // create the datagrids and the report pages dynamically, and we have to databind
                // the datagrids to the pages, and thus we have to have some references to the datagrids)
                _partB_waypointGrids.Add(pageN_waypointGrid);

                if (_routeVM.ReportPages_Waypoints != null && _routeVM.ReportPages_Waypoints.Count > i)
                {
                    // Databind this datagrid to the proper page in the printable report
                    _partB_waypointGrids[i].ItemsSource = _routeVM.ReportPages_Waypoints[i];
                }

                DataGridTextColumn pageN_columnWpNo = new DataGridTextColumn();
                pageN_columnWpNo.Header = "WP No";
                pageN_columnWpNo.Width = 26;
                Binding pageN_columnWpNoBinding = new Binding("WaypointNo");
                pageN_columnWpNoBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnWpNoBinding.Mode = BindingMode.OneWay;
                pageN_columnWpNo.Binding = pageN_columnWpNoBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnWpNo);

                DataGridTextColumn pageN_columnLatitude = new DataGridTextColumn();
                pageN_columnLatitude.Header = "Latitude";
                pageN_columnLatitude.Width = 69;
                Binding pageN_columnLatitudeBinding = new Binding("Latitude");
                pageN_columnLatitudeBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnLatitudeBinding.Mode = BindingMode.OneWay;
                pageN_columnLatitudeBinding.Converter = new PositionValueConverter();
                pageN_columnLatitudeBinding.ConverterParameter = "Latitude";
                pageN_columnLatitude.Binding = pageN_columnLatitudeBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnLatitude);

                DataGridTextColumn pageN_columnLongitude = new DataGridTextColumn();
                pageN_columnLongitude.Header = "Longitude";
                pageN_columnLongitude.Width = 73;
                Binding pageN_columnLongitudeBinding = new Binding("Longitude");
                pageN_columnLongitudeBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnLongitudeBinding.Mode = BindingMode.OneWay;
                pageN_columnLongitudeBinding.Converter = new PositionValueConverter();
                pageN_columnLongitudeBinding.ConverterParameter = "Longitude";
                pageN_columnLongitude.Binding = pageN_columnLongitudeBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnLongitude);

                DataGridTextColumn pageN_columnLandmarkBearingObject = new DataGridTextColumn();
                pageN_columnLandmarkBearingObject.Header = "LandmarkBearingObject";
                pageN_columnLandmarkBearingObject.Width = 110;
                Binding pageN_columnLandmarkBearingObjectBinding = new Binding("LandmarkAtCourseAlt_Object");
                pageN_columnLandmarkBearingObjectBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnLandmarkBearingObjectBinding.Mode = BindingMode.OneWay;
                pageN_columnLandmarkBearingObject.Binding = pageN_columnLandmarkBearingObjectBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnLandmarkBearingObject);

                DataGridTextColumn pageN_columnLandmarkBearingBearing = new DataGridTextColumn();
                pageN_columnLandmarkBearingBearing.Header = "LandmarkBearingBearing";
                pageN_columnLandmarkBearingBearing.Width = 30;
                Binding pageN_columnLandmarkBearingBearingBinding = new Binding("LandmarkAtCourseAlt_Bearing");
                pageN_columnLandmarkBearingBearingBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnLandmarkBearingBearingBinding.Mode = BindingMode.OneWay;
                pageN_columnLandmarkBearingBearing.Binding = pageN_columnLandmarkBearingBearingBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnLandmarkBearingBearing);

                DataGridTextColumn pageN_columnLandmarkBearingDistance = new DataGridTextColumn();
                pageN_columnLandmarkBearingDistance.Header = "LandmarkBearingDistance";
                pageN_columnLandmarkBearingDistance.Width = 30;
                Binding pageN_columnLandmarkBearingDistanceBinding = new Binding("LandmarkAtCourseAlt_Distance");
                pageN_columnLandmarkBearingDistanceBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnLandmarkBearingDistanceBinding.Mode = BindingMode.OneWay;
                pageN_columnLandmarkBearingDistanceBinding.StringFormat = "F1";
                pageN_columnLandmarkBearingDistance.Binding = pageN_columnLandmarkBearingDistanceBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnLandmarkBearingDistance);

                DataGridTextColumn pageN_columnCourse = new DataGridTextColumn();
                pageN_columnCourse.Header = "CourseToNextWaypoint";
                pageN_columnCourse.Width = 40;
                Binding pageN_columnCourseBinding = new Binding("CourseToNextWaypoint");
                pageN_columnCourseBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnCourseBinding.Mode = BindingMode.OneWay;
                pageN_columnCourseBinding.StringFormat = "D3";
                pageN_columnCourse.Binding = pageN_columnCourseBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnCourse);

                DataGridTextColumn pageN_columnDistance = new DataGridTextColumn();
                pageN_columnDistance.Header = "DistanceToNextWaypoint";
                pageN_columnDistance.Width = 44;
                Binding pageN_columnDistanceBinding = new Binding("DistanceToNextWaypoint");
                pageN_columnDistanceBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnDistanceBinding.Mode = BindingMode.OneWay;
                pageN_columnDistanceBinding.StringFormat = "F1";
                pageN_columnDistance.Binding = pageN_columnDistanceBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnDistance);

                DataGridTextColumn pageN_columnLegSpeed = new DataGridTextColumn();
                pageN_columnLegSpeed.Header = "Speed";
                pageN_columnLegSpeed.Width = 50;
                Binding pageN_columnLegSpeedBinding = new Binding("LegSpeedSetting");
                pageN_columnLegSpeedBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnLegSpeedBinding.Mode = BindingMode.OneWay;
                pageN_columnLegSpeedBinding.StringFormat = "F1";
                pageN_columnLegSpeed.Binding = pageN_columnLegSpeedBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnLegSpeed);

                DataGridTextColumn pageN_columnTurnRadius = new DataGridTextColumn();
                pageN_columnTurnRadius.Header = "TurnRadius";
                pageN_columnTurnRadius.Width = 40;
                Binding pageN_columnTurnRadiusBinding = new Binding("TurnRadius");
                pageN_columnTurnRadiusBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnTurnRadiusBinding.Mode = BindingMode.OneWay;
                pageN_columnTurnRadiusBinding.StringFormat = "F1";
                pageN_columnTurnRadius.Binding = pageN_columnTurnRadiusBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnTurnRadius);

                DataGridTextColumn pageN_columnTurnRate = new DataGridTextColumn();
                pageN_columnTurnRate.Header = "TurnRate";
                pageN_columnTurnRate.Width = 44;
                Binding pageN_columnTurnRateBinding = new Binding("TurnRate");
                pageN_columnTurnRateBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnTurnRateBinding.Mode = BindingMode.OneWay;
                pageN_columnTurnRateBinding.StringFormat = "F1";
                pageN_columnTurnRate.Binding = pageN_columnTurnRateBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnTurnRate);

                DataGridTextColumn pageN_columnMaxOffTrack = new DataGridTextColumn();
                pageN_columnMaxOffTrack.Header = "MaxOffTrack";
                pageN_columnMaxOffTrack.Width = 36;
                Binding pageN_columnMaxOffTrackBinding = new Binding("MaxOffTrack");
                pageN_columnMaxOffTrackBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnMaxOffTrackBinding.Mode = BindingMode.OneWay;
                pageN_columnMaxOffTrackBinding.StringFormat = "F1";
                pageN_columnMaxOffTrack.Binding = pageN_columnMaxOffTrackBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnMaxOffTrack);

                DataGridTextColumn pageN_columnPosFixMethod = new DataGridTextColumn();
                pageN_columnPosFixMethod.Header = "PosFixMethod";
                pageN_columnPosFixMethod.Width = 46;
                Binding pageN_columnPosFixMethodBinding = new Binding("PosFixMethod");
                pageN_columnPosFixMethodBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnPosFixMethodBinding.Mode = BindingMode.OneWay;
                pageN_columnPosFixMethod.Binding = pageN_columnPosFixMethodBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnPosFixMethod);

                DataGridTextColumn pageN_columnParalellIndexObject = new DataGridTextColumn();
                pageN_columnParalellIndexObject.Header = "ParalellIndexObject";
                pageN_columnParalellIndexObject.Width = 110;
                Binding pageN_columnParalellIndexObjectBinding = new Binding("LegReferenceObject_Object");
                pageN_columnParalellIndexObjectBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnParalellIndexObjectBinding.Mode = BindingMode.OneWay;
                pageN_columnParalellIndexObject.Binding = pageN_columnParalellIndexObjectBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnParalellIndexObject);

                DataGridTextColumn pageN_columnParalellIndexBearing = new DataGridTextColumn();
                pageN_columnParalellIndexBearing.Header = "ParalellIndexBearing";
                pageN_columnParalellIndexBearing.Width = 30;
                Binding pageN_columnParalellIndexBearingBinding = new Binding("LegReferenceObject_Bearing");
                pageN_columnParalellIndexBearingBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnParalellIndexBearingBinding.Mode = BindingMode.OneWay;
                pageN_columnParalellIndexBearing.Binding = pageN_columnParalellIndexBearingBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnParalellIndexBearing);

                DataGridTextColumn pageN_columnParalellIndexDistance = new DataGridTextColumn();
                pageN_columnParalellIndexDistance.Header = "ParalellIndexDistance";
                pageN_columnParalellIndexDistance.Width = 30;
                Binding pageN_columnParalellIndexDistanceBinding = new Binding("LegReferenceObject_Distance");
                pageN_columnParalellIndexDistanceBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnParalellIndexDistanceBinding.Mode = BindingMode.OneWay;
                pageN_columnParalellIndexDistance.Binding = pageN_columnParalellIndexDistanceBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnParalellIndexDistance);

                DataGridTextColumn pageN_columnRemarks = new DataGridTextColumn();
                pageN_columnRemarks.Header = "Remarks";
                pageN_columnRemarks.Width = new DataGridLength(1.0, DataGridLengthUnitType.Star);
                Binding pageN_columnRemarksBinding = new Binding("Remarks");
                pageN_columnRemarksBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnRemarksBinding.Mode = BindingMode.OneWay;
                pageN_columnRemarks.Binding = pageN_columnRemarksBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnRemarks);

                // Get the panel in which the datagrid will reside
                StackPanel waypointPanelPageN = ((StackPanel)(passagePlanBPageN.FindName("waypointPanel")));

                // Add the datagrid to the panel
                waypointPanelPageN.Children.Add(pageN_waypointGrid);

                if (i > 0)
                {
                    // Hide/Collapse the upper base information grid, which is only meant for the first page
                    Border baseInformationSection = ((Border)(passagePlanBPageN.FindName("baseInformationSection")));
                    baseInformationSection.Visibility = System.Windows.Visibility.Collapsed;
                }

                // Add the page to the fixed document
                PageContent planBPageNContent = new PageContent();
                ((IAddChild)planBPageNContent).AddChild(passagePlanBPageN);
                _fixedDoc.Pages.Add(planBPageNContent);
                _noOfPages++;

                // Add current date
                TextBlock pageN_textBlockDate = ((TextBlock)(passagePlanBPageN.FindName("textBlockDate")));
                pageN_textBlockDate.Text = System.DateTime.Now.Date.ToShortDateString();

                // Add page number
                TextBlock tb = (TextBlock)(passagePlanBPageN.FindName("textBlockPageNo"));
                tb.Text = _noOfPages.ToString();
                _textBlockPageNo_PartBPageN.Add(tb);

                // Add Passage Planner version text
                TextBlock tb2 = (TextBlock)(passagePlanBPageN.FindName("assemblyVersionText"));
                tb2.Text = GetAssemblyVersionText();
            }
        }

        /// <summary>
        /// Create the pages in the Passage Plan printable report, called "Charts and Publications...".
        /// One page if few waypoints. Otherwise more than one page.
        /// </summary>
        private void AddChartsAndPublications()
        {
            // Calculate how many pages has to be created
            double numberOfPagesDouble = 1 + Convert.ToDouble(_routeVM.NoOfWaypoints - RouteViewModel.NUMBER_OF_WAYPOINTS_IN_CHARTS_AND_PUBLICATIONS_PAGE) / Convert.ToDouble(RouteViewModel.NUMBER_OF_WAYPOINTS_IN_CHARTS_AND_PUBLICATIONS_PAGE);
            int numberOfPagePages = Convert.ToInt32(Math.Ceiling(numberOfPagesDouble));

           
            // Create a list with datagrids, one for each page
            _chartsAndPublications_waypointGrids = new List<DataGrid>();

            // Create a list with text boxes, holding page numbers
            _textBlockPageNo_ChartsAndPublications = new List<TextBlock>();
            
            for (int i = 0; i < numberOfPagePages; i++)
            {
                // Read xaml file template
                StreamReader chartsAndPublicationsPageNReader = new StreamReader(new FileStream(_reportTemplatesDirectory + "PassagePlanChartsAndPublications.xaml", FileMode.Open, FileAccess.Read));
                XmlTextReader chartsAndPublicationsPageNXmlTextReader = new XmlTextReader(chartsAndPublicationsPageNReader);
                FixedPage passagePlanChartsAndPublicationsPageN = (FixedPage)XamlReader.Load(chartsAndPublicationsPageNXmlTextReader);
                chartsAndPublicationsPageNReader.Close();

                DataGrid pageN_waypointGrid = new DataGrid();
                pageN_waypointGrid.BorderThickness = new Thickness(1, 1, 0, 0);
                pageN_waypointGrid.CanUserAddRows = false;
                pageN_waypointGrid.CanUserDeleteRows = false;
                pageN_waypointGrid.CanUserReorderColumns = false;
                pageN_waypointGrid.CanUserResizeColumns = false;
                pageN_waypointGrid.CanUserResizeRows = false;
                pageN_waypointGrid.CanUserSortColumns = false;
                pageN_waypointGrid.AutoGenerateColumns = false;
                pageN_waypointGrid.HeadersVisibility = DataGridHeadersVisibility.None;
                pageN_waypointGrid.PreviewMouseWheel += waypointGrid_PreviewMouseWheel;
                pageN_waypointGrid.FontSize = DATAGRID_FONTSIZE;
                pageN_waypointGrid.BorderBrush = Brushes.Black;



                // Add this datagrid to the list of datagrids (we put them into a list because we
                // create the datagrids and the report pages dynamically, and we have to databind
                // the datagrids to the pages, and thus we have to have some references to the datagrids)
                _chartsAndPublications_waypointGrids.Add(pageN_waypointGrid);

                if (_routeVM.ReportPages_Waypoints_ChartsAndPublications != null && _routeVM.ReportPages_Waypoints_ChartsAndPublications.Count > i)
                {
                    // Databind this datagrid to the proper page in the printable report
                    _chartsAndPublications_waypointGrids[i].ItemsSource = _routeVM.ReportPages_Waypoints_ChartsAndPublications[i];
                }

                // Allow text wrap (for most of the columns)
                Style textWrapStyle = new Style(typeof(TextBlock));
                textWrapStyle.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
                

                DataGridTextColumn pageN_columnWpNo = new DataGridTextColumn();
                pageN_columnWpNo.Header = "WP No";
                pageN_columnWpNo.Width = 26;
                Binding pageN_columnWpNoBinding = new Binding("WaypointNo");
                pageN_columnWpNoBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnWpNoBinding.Mode = BindingMode.OneWay;
                pageN_columnWpNo.Binding = pageN_columnWpNoBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnWpNo);

                DataGridTextColumn pageN_columnWpName = new DataGridTextColumn();
                pageN_columnWpName.Header = "Waypoint Name";
                pageN_columnWpName.Width = 117;
                Binding pageN_columnWpNameBinding = new Binding("WaypointName");
                pageN_columnWpNameBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnWpNameBinding.Mode = BindingMode.OneWay;
                pageN_columnWpName.Binding = pageN_columnWpNameBinding;
                pageN_waypointGrid.Columns.Add(pageN_columnWpName);

                DataGridTextColumn pageN_columnCharts = new DataGridTextColumn();
                pageN_columnCharts.Header = "Charts";
                pageN_columnCharts.Width = 150;
                Binding pageN_columnChartsBinding = new Binding("Charts");
                pageN_columnChartsBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnChartsBinding.Mode = BindingMode.OneWay;
                pageN_columnCharts.Binding = pageN_columnChartsBinding;
                pageN_columnCharts.ElementStyle = textWrapStyle;
                pageN_waypointGrid.Columns.Add(pageN_columnCharts);

                DataGridTextColumn pageN_columnListOfLights_Volume = new DataGridTextColumn();
                pageN_columnListOfLights_Volume.Header = "ListOfLights_Volume";
                pageN_columnListOfLights_Volume.Width = 85;
                Binding pageN_columnListOfLights_VolumeBinding = new Binding("ListOfLights_Volume");
                pageN_columnListOfLights_VolumeBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnListOfLights_VolumeBinding.Mode = BindingMode.OneWay;
                pageN_columnListOfLights_Volume.Binding = pageN_columnListOfLights_VolumeBinding;
                pageN_columnListOfLights_Volume.ElementStyle = textWrapStyle;
                pageN_waypointGrid.Columns.Add(pageN_columnListOfLights_Volume);

                DataGridTextColumn pageN_columnListOfLights_Page = new DataGridTextColumn();
                pageN_columnListOfLights_Page.Header = "ListOfLights_Page";
                pageN_columnListOfLights_Page.Width = 50;
                Binding pageN_columnListOfLights_PageBinding = new Binding("ListOfLights_Page");
                pageN_columnListOfLights_PageBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnListOfLights_PageBinding.Mode = BindingMode.OneWay;
                pageN_columnListOfLights_Page.Binding = pageN_columnListOfLights_PageBinding;
                pageN_columnListOfLights_Page.ElementStyle = textWrapStyle;
                pageN_waypointGrid.Columns.Add(pageN_columnListOfLights_Page);

                DataGridTextColumn pageN_columnListOfRadioSignals_Volume = new DataGridTextColumn();
                pageN_columnListOfRadioSignals_Volume.Header = "ListOfRadioSignals_Volume";
                pageN_columnListOfRadioSignals_Volume.Width = 85;
                Binding pageN_columnListOfRadioSignals_VolumeBinding = new Binding("ListOfRadioSignals_Volume");
                pageN_columnListOfRadioSignals_VolumeBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnListOfRadioSignals_VolumeBinding.Mode = BindingMode.OneWay;
                pageN_columnListOfRadioSignals_Volume.Binding = pageN_columnListOfRadioSignals_VolumeBinding;
                pageN_columnListOfRadioSignals_Volume.ElementStyle = textWrapStyle;
                pageN_waypointGrid.Columns.Add(pageN_columnListOfRadioSignals_Volume);

                DataGridTextColumn pageN_columnListOfRadioSignals_Page = new DataGridTextColumn();
                pageN_columnListOfRadioSignals_Page.Header = "ListOfRadioSignals_Page";
                pageN_columnListOfRadioSignals_Page.Width = 50;
                Binding pageN_columnListOfRadioSignals_PageBinding = new Binding("ListOfRadioSignals_Page");
                pageN_columnListOfRadioSignals_PageBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnListOfRadioSignals_PageBinding.Mode = BindingMode.OneWay;
                pageN_columnListOfRadioSignals_Page.Binding = pageN_columnListOfRadioSignals_PageBinding;
                pageN_columnListOfRadioSignals_Page.ElementStyle = textWrapStyle;
                pageN_waypointGrid.Columns.Add(pageN_columnListOfRadioSignals_Page);

                DataGridTextColumn pageN_columnSailingDirections_Volume = new DataGridTextColumn();
                pageN_columnSailingDirections_Volume.Header = "SailingDirections_Volume";
                pageN_columnSailingDirections_Volume.Width = 85;
                Binding pageN_columnSailingDirections_VolumeBinding = new Binding("SailingDirections_Volume");
                pageN_columnSailingDirections_VolumeBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnSailingDirections_VolumeBinding.Mode = BindingMode.OneWay;
                pageN_columnSailingDirections_Volume.Binding = pageN_columnSailingDirections_VolumeBinding;
                pageN_columnSailingDirections_Volume.ElementStyle = textWrapStyle;
                pageN_waypointGrid.Columns.Add(pageN_columnSailingDirections_Volume);

                DataGridTextColumn pageN_columnSailingDirections_Page = new DataGridTextColumn();
                pageN_columnSailingDirections_Page.Header = "SailingDirections_Page";
                pageN_columnSailingDirections_Page.Width = 50;
                Binding pageN_columnSailingDirections_PageBinding = new Binding("SailingDirections_Page");
                pageN_columnSailingDirections_PageBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnSailingDirections_PageBinding.Mode = BindingMode.OneWay;
                pageN_columnSailingDirections_Page.Binding = pageN_columnSailingDirections_PageBinding;
                pageN_columnSailingDirections_Page.ElementStyle = textWrapStyle;
                pageN_waypointGrid.Columns.Add(pageN_columnSailingDirections_Page);

                DataGridTextColumn pageN_columnNavtexChannels = new DataGridTextColumn();
                pageN_columnNavtexChannels.Header = "NavtexChannels";
                pageN_columnNavtexChannels.Width = 70;
                Binding pageN_columnNavtexChannelsBinding = new Binding("NavtexChannels");
                pageN_columnNavtexChannelsBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnNavtexChannelsBinding.Mode = BindingMode.OneWay;
                pageN_columnNavtexChannels.Binding = pageN_columnNavtexChannelsBinding;
                pageN_columnNavtexChannels.ElementStyle = textWrapStyle;
                pageN_waypointGrid.Columns.Add(pageN_columnNavtexChannels);

                DataGridTextColumn pageN_columnReportTo = new DataGridTextColumn();
                pageN_columnReportTo.Header = "ReportTo";
                pageN_columnReportTo.Width = 140;
                Binding pageN_columnReportToBinding = new Binding("ReportTo");
                pageN_columnReportToBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnReportToBinding.Mode = BindingMode.OneWay;
                pageN_columnReportTo.Binding = pageN_columnReportToBinding;
                pageN_columnReportTo.ElementStyle = textWrapStyle;
                pageN_waypointGrid.Columns.Add(pageN_columnReportTo);

                DataGridTextColumn pageN_columnChannelOrTelephoneNo = new DataGridTextColumn();
                pageN_columnChannelOrTelephoneNo.Header = "ChannelOrTelephoneNo";
                pageN_columnChannelOrTelephoneNo.Width = 110;

                Binding pageN_columnChannelOrTelephoneNoBinding = new Binding("ChannelOrTelephoneNo");
                pageN_columnChannelOrTelephoneNoBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                pageN_columnChannelOrTelephoneNoBinding.Mode = BindingMode.OneWay;
                pageN_columnChannelOrTelephoneNo.Binding = pageN_columnChannelOrTelephoneNoBinding;
                pageN_columnChannelOrTelephoneNo.ElementStyle = textWrapStyle;
                pageN_waypointGrid.Columns.Add(pageN_columnChannelOrTelephoneNo);

                // Get the panel in which the datagrid will reside
                StackPanel waypointPanelPageN = ((StackPanel)(passagePlanChartsAndPublicationsPageN.FindName("waypointPanel")));

                // Add the datagrid to the panel
                waypointPanelPageN.Children.Add(pageN_waypointGrid);

                if (i > 0)
                {
                    // Hide/Collapse the upper base information grid, which is only meant for the first page
                    Border baseInformationSection = ((Border)(passagePlanChartsAndPublicationsPageN.FindName("baseInformationSection")));
                    baseInformationSection.Visibility = System.Windows.Visibility.Collapsed;
                }

                // Add the page to the fixed document
                PageContent chartsAndPublicationsPageNContent = new PageContent();
                ((IAddChild)chartsAndPublicationsPageNContent).AddChild(passagePlanChartsAndPublicationsPageN);
                _fixedDoc.Pages.Add(chartsAndPublicationsPageNContent);
                _noOfPages++;

                // Add current date
                TextBlock pageN_textBlockDate = ((TextBlock)(passagePlanChartsAndPublicationsPageN.FindName("textBlockDate")));
                pageN_textBlockDate.Text = System.DateTime.Now.Date.ToShortDateString();

                // Add page number
                TextBlock tb = (TextBlock)(passagePlanChartsAndPublicationsPageN.FindName("textBlockPageNo"));
                tb.Text = _noOfPages.ToString();
                _textBlockPageNo_ChartsAndPublications.Add(tb);
                
                // Add Passage Planner version text
                TextBlock tb2 = (TextBlock)(passagePlanChartsAndPublicationsPageN.FindName("assemblyVersionText"));
                tb2.Text = GetAssemblyVersionText();
            }
        }

        private void AddAdditionalNotesPage()
        {
            StreamReader additionalNotesReader = new StreamReader(new FileStream(_reportTemplatesDirectory + "PassagePlanAdditionalNotes.xaml", FileMode.Open, FileAccess.Read));
            XmlTextReader additionalNotesXmlTextReader = new XmlTextReader(additionalNotesReader);
            FixedPage additionalNotesPage = (FixedPage)XamlReader.Load(additionalNotesXmlTextReader);
            additionalNotesReader.Close();
            PageContent additionalNotesContent = new PageContent();
            ((IAddChild)additionalNotesContent).AddChild(additionalNotesPage);
            _fixedDoc.Pages.Add(additionalNotesContent);
            _noOfPages++;

            TextBlock textBlockDate = ((TextBlock)(additionalNotesPage.FindName("textBlockDate")));
            textBlockDate.Text = System.DateTime.Now.Date.ToShortDateString();

            _textBlockPageNo_AdditionalNotes = ((TextBlock)(additionalNotesPage.FindName("textBlockPageNo")));
            _textBlockPageNo_AdditionalNotes.Text = _noOfPages.ToString();

            // Add Passage Planner version text
            TextBlock tb = (TextBlock)(additionalNotesPage.FindName("assemblyVersionText"));
            tb.Text = GetAssemblyVersionText();

            _textBoxAdditionalNotes = ((TextBox)(additionalNotesPage.FindName("textBoxAdditionalNotes")));
            _textBoxAdditionalNotes.PreviewKeyDown += textBox_PreviewKeyDown_max32Lines;
        }


        private string GetAssemblyVersionText()
        {
            About about = new About();
            return "Printed from Seaware Passage Planner version " + about.AssemblyVersion + ". " + about.AssemblyCopyright + ". All Rights Reserved.";
        }

        /// <summary>
        /// This event handler fixes the "bug" where you couldn't scroll the document
        /// if the mouse was over the datagrid. 
        /// The scroll movement is instead transferred to the scrollviewer of the documentviewer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void waypointGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Get the scroll viewer of documentViewer
            ScrollViewer dvScrollViewer = documentViewer.Template.FindName("PART_ContentHost", documentViewer) as ScrollViewer;
            dvScrollViewer.ScrollToVerticalOffset(dvScrollViewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        /// <summary>
        /// After all pages are created we can call this method and add the
        /// the short text containing the total number of pages in parenthesis.
        /// </summary>
        private void SetTotalNumberOfPages()
        {
            if (_textBlockPageNo_GeneralNotesPart1 != null)
            {
                _textBlockPageNo_GeneralNotesPart1.Text += " (" + _noOfPages + ")";
            }
            if (_textBlockPageNo_GeneralNotesPart2 != null)
            {
                _textBlockPageNo_GeneralNotesPart2.Text += " (" + _noOfPages + ")";
            }
            if (_textBlockPageNo_PartAPageN != null)
            {
                foreach (TextBlock tb in _textBlockPageNo_PartAPageN)
                {
                    tb.Text += " (" + _noOfPages + ")";
                }
            }
            if (_textBlockPageNo_PartBPageN != null)
            {
                foreach (TextBlock tb in _textBlockPageNo_PartBPageN)
                {
                    tb.Text += " (" + _noOfPages + ")";
                }
            }
            if (_textBlockPageNo_ChartsAndPublications != null)
            {
                foreach (TextBlock tb in _textBlockPageNo_ChartsAndPublications)
                {
                    tb.Text += " (" + _noOfPages + ")";
                }
            }
            if (_textBlockPageNo_AdditionalNotes != null)
            {
                _textBlockPageNo_AdditionalNotes.Text += " (" + _noOfPages + ")";
            }
        }

        /// <summary>
        /// Limit number of lines to 4, so no lines will be invisible in the printout if users writes too much.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_PreviewKeyDown_max4Lines(object sender, KeyEventArgs e)
        {
            int maxNumberOfLines = 4;
            if (sender.GetType() == typeof(TextBox))
            {
                TextBox textBox = (TextBox)sender;
                int caretPosition = textBox.SelectionStart;

                if (e.Key == Key.Enter && textBox.LineCount > maxNumberOfLines - 1)
                {
                    e.Handled = true;
                }
                // Ignore following keys
                else if (e.Key == Key.Back || e.Key == Key.Tab || e.Key == Key.Delete || e.Key == Key.Left || e.Key == Key.Right ||
                    e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.End || e.Key == Key.Home || e.Key == Key.PageUp ||
                    e.Key == Key.PageDown || e.Key == Key.LeftShift || e.Key == Key.RightShift || e.Key == Key.CapsLock ||
                    e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl || e.Key == Key.C)
                {
                    e.Handled = false;
                }
                else
                {
                    string originalText = textBox.Text;
                    // Try to add one character, and then test if it is too many lines
                    string testText = originalText + "A";
                    textBox.Text = testText;
                    if (textBox.LineCount > maxNumberOfLines)
                    {
                        e.Handled = true;
                    }
                    textBox.Text = originalText;
                    try
                    {
                        // Move cursor to original position
                        textBox.SelectionStart = caretPosition;
                    }
                    catch (Exception ex)
                    {
                        // Do nothing
                    }
                }
            }
        }

        /// <summary>
        /// Limit number of lines to 7, so no lines will be invisible in the printout if users writes too much.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_PreviewKeyDown_max7Lines(object sender, KeyEventArgs e)
        {
            int maxNumberOfLines = 7;
            if (sender.GetType() == typeof(TextBox))
            {
                TextBox textBox = (TextBox)sender;
                int caretPosition = textBox.SelectionStart;

                if (e.Key == Key.Enter && textBox.LineCount > maxNumberOfLines - 1)
                {
                    e.Handled = true;
                }
                // Ignore following keys
                else if (e.Key == Key.Back || e.Key == Key.Tab || e.Key == Key.Delete || e.Key == Key.Left || e.Key == Key.Right ||
                    e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.End || e.Key == Key.Home || e.Key == Key.PageUp ||
                    e.Key == Key.PageDown || e.Key == Key.LeftShift || e.Key == Key.RightShift || e.Key == Key.CapsLock ||
                    e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl || e.Key == Key.C)
                {
                    e.Handled = false;
                }
                else
                {
                    string originalText = textBox.Text;
                    // Try to add one character, and then test if it is too many lines
                    string testText = originalText + "A";
                    textBox.Text = testText;
                    if (textBox.LineCount > maxNumberOfLines)
                    {
                        e.Handled = true;
                    }
                    textBox.Text = originalText;
                    try
                    {
                        // Move cursor to original position
                        textBox.SelectionStart = caretPosition;
                    }
                    catch (Exception ex)
                    {
                        // Do nothing
                    }
                }
            }
        }

        /// <summary>
        /// Limit number of lines to 32, so no lines will be invisible in the printout if users writes too much.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_PreviewKeyDown_max32Lines(object sender, KeyEventArgs e)
        {
            int maxNumberOfLines = 32;
            if (sender.GetType() == typeof(TextBox))
            {
                TextBox textBox = (TextBox)sender;
                int caretPosition = textBox.SelectionStart;

                if (e.Key == Key.Enter && textBox.LineCount > maxNumberOfLines - 1)
                {
                    e.Handled = true;
                }
                // Ignore following keys
                else if (e.Key == Key.Back || e.Key == Key.Tab || e.Key == Key.Delete || e.Key == Key.Left || e.Key == Key.Right ||
                    e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.End || e.Key == Key.Home || e.Key == Key.PageUp ||
                    e.Key == Key.PageDown || e.Key == Key.LeftShift || e.Key == Key.RightShift || e.Key == Key.CapsLock ||
                    e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl || e.Key == Key.C)
                {
                    e.Handled = false;
                }
                else
                {
                    string originalText = textBox.Text;
                    // Try to add one character, and then test if it is too many lines
                    string testText = originalText + "A";
                    textBox.Text = testText;
                    if (textBox.LineCount > maxNumberOfLines)
                    {
                        e.Handled = true;
                    }
                    textBox.Text = originalText;
                    try
                    {
                        // Move cursor to original position
                        textBox.SelectionStart = caretPosition;
                    }
                    catch (Exception ex)
                    {
                        // Do nothing
                    }
                }
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _textBoxGeneral.PreviewKeyDown -= textBox_PreviewKeyDown_max7Lines;

            _textBoxNavigationalWarnings.PreviewKeyDown -= textBox_PreviewKeyDown_max4Lines;
            _textBoxEmergencyAnchorages.PreviewKeyDown -= textBox_PreviewKeyDown_max4Lines;
            _textBoxTnPNotices.PreviewKeyDown -= textBox_PreviewKeyDown_max4Lines;
            _textBoxWeather.PreviewKeyDown -= textBox_PreviewKeyDown_max4Lines;
            _textBoxRestrictedAreas.PreviewKeyDown -= textBox_PreviewKeyDown_max4Lines;
            _textBoxRemarks.PreviewKeyDown -= textBox_PreviewKeyDown_max4Lines;

            _textBoxAdditionalNotes.PreviewKeyDown -= textBox_PreviewKeyDown_max32Lines;

            documentViewer.Document = null;
            _fixedDoc = null;
            _textBlockPageNo_GeneralNotesPart1 = null;
            _textBlockPageNo_GeneralNotesPart2 = null;
            _textBlockPageNo_PartAPageN = null;
            _textBlockPageNo_PartBPageN = null;
            _textBlockPageNo_ChartsAndPublications = null;
        }
    }
}
