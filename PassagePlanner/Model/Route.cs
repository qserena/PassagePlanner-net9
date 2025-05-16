using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelLayer;
using System.Windows.Media;
using System.IO;
using System.Xml.Serialization;
using System.Windows;
using System.Xml;
using EcdisLayer;
using Seaware.Navigation.Enumerations;

namespace PassagePlanner
{
    [Serializable]
    public class Route
    {
        private string _fileName = string.Empty;
        private PassagePlanManuallyAddedTexts _passagePlanTexts = new PassagePlanManuallyAddedTexts();
        

        public string DeparturePort { get; set; }
        public string ArrivalPort { get; set; }
        public string RouteName { get; set; }
        public double TimeZoneDeparture { get; set; }
        public double TimeZoneArrival { get; set; }
        public DateTime? DepartureTimeDate { get; set; }
        public DateTime? DepartureTimeHoursAndMinutes { get; set; }
        public double DraughtDepartureFore { get; set; }
        public double DraughtDepartureAft { get; set; }
        public double DraughtDepartureAir { get; set; }
        public double DraughtArrivalFore { get; set; }
        public double DraughtArrivalAft { get; set; }
        public double DraughtArrivalAir { get; set; }
        public DispatchingObservableCollection<Waypoint> Waypoints { get; set; }
        public int SelectedSospWaypointIndex { get; set; }
        public int SelectedEospWaypointIndex { get; set; }

        // This flag was introduced to let Passage Planner open a Passage Planner file (*.ppf) WITHOUT WAYPOINTS.
        // (could be good if user starts with entering for example Departure port and Arrival port, and the wants to save before adding waypoints).
        public bool PassagePlannerFileOpenOK { get; set; }


        /// <summary>
        /// Parameter-less constructor needed for serialization to work
        /// </summary>
        public Route()
        {
            SelectedSospWaypointIndex = -1;
            SelectedEospWaypointIndex = -1;
        }

        public Route(string fileName)
        {
            SelectedSospWaypointIndex = -1;
            SelectedEospWaypointIndex = -1;

            PassagePlannerFileOpenOK = false;
            this.GetRoute(fileName, false);
        }

        /// <summary>
        /// Constructor for Ecdis import
        /// </summary>
        /// <param name="routeInfo"></param>
        public Route(TSw_EcdisImportAndExportRouteInfoType routeInfo)
        {
            SelectedSospWaypointIndex = -1;
            SelectedEospWaypointIndex = -1;

            if (this.Waypoints == null)
            {
                this.Waypoints = new DispatchingObservableCollection<Waypoint>();
            }

            this.DepartureTimeDate = routeInfo.etd.Date;
            this.DepartureTimeHoursAndMinutes = DateTime.MinValue.AddHours(routeInfo.etd.Hour).AddMinutes(routeInfo.etd.Minute);
            this.DeparturePort = string.Empty;
            this.ArrivalPort = string.Empty;
            this.TimeZoneDeparture = 0;
            this.TimeZoneArrival = 0;
        }

        public void SetWaypoints(List<TSw_EcdisImportAndExportLegWaypointType> ecdisWaypoints)
        {
            foreach (TSw_EcdisImportAndExportLegWaypointType wp in ecdisWaypoints)
            {
                this.Waypoints.Add(new Waypoint(wp));
            }
        }

        public string FileName
        {
            get
            {
                return _fileName;
            }

            set 
            {
                if (_fileName != value)
                {
                    _fileName = value;
                }
            }
        }

        public PassagePlanManuallyAddedTexts PassagePlanTexts
        {
            get
            {
                return _passagePlanTexts;
            }

            set
            {
                if (_passagePlanTexts != value)
                {
                    _passagePlanTexts = value;
                }
            }
        }

        public void AppendRoute(string fileName)
        {
            this.GetRoute(fileName, true);
        }

        public void GetRoute(string fileName, bool append)
        {
            if (fileName != string.Empty)
            {

                this.Waypoints = new DispatchingObservableCollection<Waypoint>();

                
                if (fileName.EndsWith(FileManager.RouteExtension))
                {
                    // Passage Planner Route format
                    AddPassagePlannerFile(fileName, append);
                }
                else if (fileName.EndsWith(".swx"))
                {
                    // Seaware format
                    AddSeawareFormatFile(fileName, append);
                }
                else if (Path.GetExtension(fileName) == ".xlsx")
                {
                    MessageBox mb = new MessageBox("File type *.xlsx is not supported.\n\nPlease open file in Excel and save as file type *.xls.\nThen open the *.xls file in Passage Planner.", "File type not supported", MessageBoxButton.OK);
                    mb.ShowDialog();
                }
                else if (fileName.EndsWith(".xls"))
                {
                    // Old NaviPlan Excel format (Office 97-2003)
                    AddExcelFile(fileName, append);
                }
                else
                {
                    // Try to open with Ecdis plugin
                    EcdisPluginHandler ecdisHandler = new EcdisPluginHandler();
                    List<string> availablePlugins = ecdisHandler.GetAvailablePlugins();

                    if (availablePlugins.Count == 0)
                    {
                        return;
                    }

                    bool success = false;
                    foreach (string pluginName in availablePlugins)
                    {
                        IEcdisPlugin plugin = ecdisHandler.GetPlugin(pluginName);
                        List<string> supportedFileExtensions = plugin.GetSupportedFileTypes();
                        foreach (string fileExtension in supportedFileExtensions)
                        {
                            if (fileName.EndsWith(fileExtension) || fileName.EndsWith(fileExtension.ToUpper()))
                            {
                                // Try to read Ecdis
                               
                                try
                                {
                                    success = plugin.ReadRouteFile(fileName);
                                }
                                catch (Exception ex)
                                {
                                    // There might be an exception depending on reading the file
                                    // with the wrong type of Ecdis plugin - The same file extension can
                                    // be supported by two or more Ecdis plugins.
                                    // So, don't worry. Just eat the exception and continue with the next plugin!
                                }

                                if (success)
                                {
                                    TSw_EcdisImportAndExportRouteInfoType routeInfo = plugin.FRouteInfo;
                                    Route ecdisRoute = new Route(routeInfo);
                                    ecdisRoute.SetWaypoints(plugin.FWaypoints);

                                    ArrivalPort = ecdisRoute.ArrivalPort;
                                    TimeZoneArrival = ecdisRoute.TimeZoneArrival;
                                    SelectedEospWaypointIndex = ecdisRoute.SelectedEospWaypointIndex;

                                    if (!append)
                                    {
                                        // These properties is only set if you open a NEW file, not if you append a file to an already existing file.
                                        DeparturePort = ecdisRoute.DeparturePort;
                                        TimeZoneDeparture = ecdisRoute.TimeZoneDeparture;
                                        SelectedSospWaypointIndex = ecdisRoute.SelectedSospWaypointIndex;
                                        DepartureTimeDate = ecdisRoute.DepartureTimeDate;
                                        DepartureTimeHoursAndMinutes = ecdisRoute.DepartureTimeHoursAndMinutes;

                                        if (DeparturePort != null && DeparturePort.Length == 0 && 
                                            ArrivalPort != null && ArrivalPort.Length == 0 &&
                                            routeInfo.routeId != null && routeInfo.routeId.Length > 0)
                                        {
                                            RouteName = routeInfo.routeId;
                                        }
                                    }

                                    foreach (Waypoint wp in ecdisRoute.Waypoints)
                                    {
                                        this.Waypoints.Add(wp);
                                    }

                                    return;
                                }
                            }
                        }
                    }
                    // We have tested everything - Give up!
                    throw new Exception("File " + fileName + " could not be opened.");
                }

            }
        }

        private void AddSeawareFormatFile(string fileName, bool append)
        {
            activeRoute activeRoute = null;
            FileStream myFileStream = null;
            try
            {
                XmlSerializer mySerializer2 = new XmlSerializer(typeof(activeRoute));
                myFileStream = new FileStream(fileName, FileMode.Open);
                activeRoute = (activeRoute)mySerializer2.Deserialize(myFileStream);
            }
            catch (Exception ex)
            {
                ErrorHandler.Show(ex);
            }
            finally
            {
                if (myFileStream != null)
                {
                    myFileStream.Close();
                }
            }

            if (activeRoute != null)
            {
                foreach (activeRouteRouteWp waypoint in activeRoute.routeWp)
                {
                    LegType legType = (waypoint.followingLegType.ToLower() == "gc" ? LegType.GreatCircle : LegType.RhumbLine);
                    double? speed = (waypoint.resultingSpeedInKnotSpecified ? (double?)waypoint.resultingSpeedInKnot : null);
                    this.Waypoints.Add(new Waypoint(waypoint.latAsDecimalDegree, waypoint.longAsDecimalDegree, legType, speed));
                }
            }
        }

        private Route OpenPassagePlannerFormat(string filePath)
        {
            FileStream myFileStream = null;
            try
            {
                XmlSerializer mySerializer = new XmlSerializer(typeof(Route));
                myFileStream = new FileStream(filePath, FileMode.Open);
                return (Route)mySerializer.Deserialize(myFileStream);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                myFileStream.Close();
            }
        }

        private void AddPassagePlannerFile(string fileName, bool append)
        {
            try
            {
                Route passagePlannerRoute = OpenPassagePlannerFormat(fileName);

                if (passagePlannerRoute.Waypoints != null)
                {
                    foreach (Waypoint wp in passagePlannerRoute.Waypoints)
                    {
                        this.Waypoints.Add(wp);
                    }
                }

                if (!append)
                {
                    this.DeparturePort = passagePlannerRoute.DeparturePort;
                    this.TimeZoneDeparture = passagePlannerRoute.TimeZoneDeparture;
                    this.DepartureTimeDate = passagePlannerRoute.DepartureTimeDate;
                    this.DepartureTimeHoursAndMinutes = passagePlannerRoute.DepartureTimeHoursAndMinutes;
                    this.PassagePlanTexts = passagePlannerRoute.PassagePlanTexts;
                    this.DraughtArrivalAft = passagePlannerRoute.DraughtArrivalAft;
                    this.DraughtArrivalAir = passagePlannerRoute.DraughtArrivalAir;
                    this.DraughtArrivalFore = passagePlannerRoute.DraughtArrivalFore;
                    this.DraughtDepartureAft = passagePlannerRoute.DraughtDepartureAft;
                    this.DraughtDepartureAir = passagePlannerRoute.DraughtDepartureAir;
                    this.DraughtDepartureFore = passagePlannerRoute.DraughtDepartureFore;
                    this.SelectedSospWaypointIndex = passagePlannerRoute.SelectedSospWaypointIndex;
                }

                this.ArrivalPort = passagePlannerRoute.ArrivalPort;
                this.TimeZoneArrival = passagePlannerRoute.TimeZoneArrival;
                this.SelectedEospWaypointIndex = passagePlannerRoute.SelectedEospWaypointIndex;

                PassagePlannerFileOpenOK = true;
            }
            catch (Exception ex)
            {
                MessageBox mb = new MessageBox("Can not read Passage Planner file " + fileName, "Error", MessageBoxButton.OK);
                mb.ShowDialog();
            }
        }

        private void AddExcelFile(string fileName, bool append)
        {
            object[][] excelData = null;
            try
            {
                Reader reader = new Reader();
                excelData = reader.GetExcelData(fileName);

                if (!append)
                {
                    this.DeparturePort = (string)excelData[0][0];

                    if (excelData[2][2] != null)
                    {
                        // This code handles TIME ZONE DEPARTURE in both Text format and Number format in Excel 
                        // (needed because when .xlsx files are saved as .xls this is often in Text format)
                        string timeZoneDepartureString = excelData[2][2].ToString();
                        this.TimeZoneDeparture = Convert.ToDouble(timeZoneDepartureString);
                    }
                }

                this.ArrivalPort = (string)excelData[0][2];

                if (excelData[3][2] != null)
                {
                    // This code handles TIME ZONE ARRIVAL in both Text format and Number format in Excel 
                    // (needed because when .xlsx files are saved as .xls this is often in Text format)
                    string timeZoneArrivalString = excelData[3][2].ToString();
                    this.TimeZoneArrival = Convert.ToDouble(timeZoneArrivalString);
                }

                int rowNumber = 7;

                while (excelData[rowNumber][0] != null)
                {
                    string wayPointName = (string)excelData[rowNumber][0];
                    string latitudeLetter = (string)excelData[rowNumber][1];
                    int latitudeDegrees = Convert.ToInt32(excelData[rowNumber][2]);
                    double latitudeMinutes = (excelData[rowNumber][3] == null ? 0.0 : (double)excelData[rowNumber][3]);
                    string longitudeLetter = (string)excelData[rowNumber][4];
                    int longitudeDegrees = Convert.ToInt32(excelData[rowNumber][5]);
                    double longitudeMinutes = (excelData[rowNumber][6] == null ? 0.0 : (double)excelData[rowNumber][6]);
                    double speed = (excelData[rowNumber][7] == null ? 0.0 : (double)excelData[rowNumber][7]);
                    string remarks = (excelData[rowNumber][8] == null ? string.Empty : excelData[rowNumber][8].ToString());
                    double minDepth = (excelData[rowNumber][9] == null ? 0.0 : (double)excelData[rowNumber][9]);
                    string charts = (excelData[rowNumber][10] == null ? string.Empty : excelData[rowNumber][10].ToString());
                    string legReferenceObject_Object = (excelData[rowNumber][11] == null ? string.Empty : excelData[rowNumber][11].ToString());
                    string legReferenceObject_Bearing = (excelData[rowNumber][12] == null ? string.Empty : excelData[rowNumber][12].ToString());
                    var i = excelData[rowNumber][13];
                    string legReferenceObject_Distance = (excelData[rowNumber][13] == null ? string.Empty : excelData[rowNumber][13].ToString());
                    double turnRadius = (excelData[rowNumber][14] == null ? 0.0 : (double)excelData[rowNumber][14]);
                    string turnRate = ((excelData[rowNumber][15] == null || excelData[rowNumber][15].ToString().Length == 0) ? string.Empty : excelData[rowNumber][15].ToString());
                    string landmarkAtCourseAlt_Object = (excelData[rowNumber][16] == null ? string.Empty : excelData[rowNumber][16].ToString());
                    string landmarkAtCourseAlt_Bearing = (excelData[rowNumber][17] == null ? string.Empty : excelData[rowNumber][17].ToString());
                    string landmarkAtCourseAlt_Distance = (excelData[rowNumber][18] == null ? string.Empty : excelData[rowNumber][18].ToString());
                    string maxOffTrack = (excelData[rowNumber][19] == null ? string.Empty : excelData[rowNumber][19].ToString());
                    string maxIntervalsPosFix = (excelData[rowNumber][20] == null ? string.Empty : excelData[rowNumber][20].ToString());
                    string engineStatus = (excelData[rowNumber][21] == null ? string.Empty : excelData[rowNumber][21].ToString());
                    string navWatchLevel = (excelData[rowNumber][22] == null ? string.Empty : excelData[rowNumber][22].ToString());
                    string posFixMethod = (excelData[rowNumber][23] == null ? string.Empty : excelData[rowNumber][23].ToString());

                    string listOfLights_Volume = ((excelData[rowNumber][24] == null || excelData[rowNumber][24].ToString().Length == 0) ? string.Empty : excelData[rowNumber][24].ToString());
                    string listOfLights_Page = (excelData[rowNumber][25] == null ? string.Empty : excelData[rowNumber][25].ToString());
                    string listOfRadioSignals_Volume = (excelData[rowNumber][26] == null ? string.Empty : excelData[rowNumber][26].ToString());
                    string listOfRadioSignals_Page = (excelData[rowNumber][27] == null ? string.Empty : excelData[rowNumber][27].ToString());
                    string sailingDirections_Volume = (excelData[rowNumber][28] == null ? string.Empty : excelData[rowNumber][28].ToString());
                    string sailingDirections_Page = (excelData[rowNumber][29] == null ? string.Empty : excelData[rowNumber][29].ToString());
                    string navtexChannels = (excelData[rowNumber][30] == null ? string.Empty : excelData[rowNumber][30].ToString());
                    string reportTo = (excelData[rowNumber][31] == null ? string.Empty : excelData[rowNumber][31].ToString());
                    string channelOrTelephoneNo = (excelData[rowNumber][32] == null ? string.Empty : excelData[rowNumber][32].ToString());
                    string actualPassingTime = string.Empty;
                    if (excelData[rowNumber].Length > 34) { actualPassingTime = (excelData[rowNumber][34] == null ? string.Empty : excelData[rowNumber][34].ToString()); }
                    
                    // GeneralNotes can still exist in this type of old NaviPlan excel file, but is removed 2014-07-21 from Passage Planner
                    // So,"generalNotes" is not used, nor passed on to class Waypoint.
                    string generalNotes = string.Empty;
                    if (excelData[rowNumber].Length > 36) { generalNotes = (excelData[rowNumber][36] == null ? string.Empty : excelData[rowNumber][36].ToString()); }
                    
                    string securityLevel = string.Empty;
                    if (excelData[rowNumber].Length > 38) { securityLevel = (excelData[rowNumber][38] == null ? string.Empty : excelData[rowNumber][38].ToString()); }

                    double latitude = PositionValueConverter.LatitudeStringToDouble(latitudeLetter + " " + latitudeDegrees + " " + latitudeMinutes);
                    double longitude = PositionValueConverter.LongitudeStringToDouble(longitudeLetter + " " + longitudeDegrees + " " + longitudeMinutes);

                    this.Waypoints.Add(
                        new Waypoint(
                            wayPointName,
                            latitude,
                            longitude,
                            speed,
                            remarks,
                            minDepth,
                            charts,
                            legReferenceObject_Object,
                            legReferenceObject_Bearing,
                            legReferenceObject_Distance,
                            turnRadius,
                            turnRate,
                            landmarkAtCourseAlt_Object,
                            landmarkAtCourseAlt_Bearing,
                            landmarkAtCourseAlt_Distance,
                            maxOffTrack,
                            maxIntervalsPosFix,
                            engineStatus,
                            navWatchLevel,
                            posFixMethod,
                            listOfLights_Volume,
                            listOfLights_Page,
                            listOfRadioSignals_Volume,
                            listOfRadioSignals_Page,
                            sailingDirections_Volume,
                            sailingDirections_Page,
                            navtexChannels,
                            reportTo,
                            channelOrTelephoneNo,
                            actualPassingTime,
                            securityLevel));

                    rowNumber++;
                }
            }
            catch (Exception ex)
            {
                MessageBox mb = new MessageBox("Error when reading Excel file.", "Error", MessageBoxButton.OK);
                mb.ShowDialog();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>New fileName (with path)</returns>
        public string Save(string fileName)
        {
            string savedFilePath = string.Empty;

            Stream fs = null;
            XmlWriter writer = null;

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Route));

                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                xmlWriterSettings.NewLineOnAttributes = true;
                xmlWriterSettings.Indent = true;

                // Create an XmlTextWriter using a FileStream.
                fs = new FileStream(fileName, FileMode.Create);

                writer = XmlTextWriter.Create(fs, xmlWriterSettings);

                // Serialize using the XmlTextWriter.
                serializer.Serialize(writer, this);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            finally
            {
                writer.Close();
                fs.Close();
            }

            FileManager.GrantAccess(fileName);

            savedFilePath = fileName;

            return savedFilePath;
        }

    }
}
