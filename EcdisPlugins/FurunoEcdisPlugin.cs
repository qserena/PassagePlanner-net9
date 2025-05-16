using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.IO;
using EcdisLayer;
using System.Globalization;

namespace MultiEcdisPlugin
{
    [Export(typeof(EcdisLayer.IEcdisPlugin))]
    [ExportMetadata("EcdisName", "Furuno")]
    public class FurunoEcdisPlugin : EcdisLayer.IEcdisPlugin
    {
        const string FRF_HEADER1 = "MAX_POWER	SPEED	ETDWPNO	ETAWPNO	OPTIMIZED	BUDGET";
        const string FRF_HEADER2 = "ETD_YEAR	ETD_MONTH	ETD_DAY	ETD_HOUR	ETD_MINUTE	ETA_YEAR	ETA_MONTH	ETA_DAY			ETA_HOUR	ETA_MINUTE";
        const string FRF_HEADER3 = "NAME	LAT	LON	LEG_TYPE	TURN_RADIUS	CHN_LIMIT	PLANNED_SPEED	SPEED_MIN			SPEED_MAX	COURSE	LENGTH		DO_PLAN	HFO_PLAN	HFO_LEFT	DO_LEFT	ETA_DAY	ETA_TIME";
        const string FRF_RL = "RHUMBLINE";
        const string FRF_GC = "GREATCIRCLE";

        public TSw_EcdisImportAndExportRouteInfoType FRouteInfo { get; set; }
        public List<TSw_EcdisImportAndExportLegWaypointType> FWaypoints { get; set; }

        public bool CanImport() { return true; }
        public bool CanExport() { return false; }
        public string GetMaker() { return "Furuno"; }
        public int GetVersion() { return 20000; } // 00.02.00.00
        public List<string> GetSupportedFileTypes() { return new List<string>() { ".txt" }; }
        public bool IsSupportedFileType(string extension) { return extension.ToLower().EndsWith("txt"); }

        NumberFormatInfo _formatProvider = new NumberFormatInfo();

        public bool ReadRouteFile(string filePath) 
        {
            bool returnValue = false;
            string currentLine;
            string waypointName;
            double lat;
            double lon;
            string legType;
            double turnRadius;
            double speed;
            
            DateTime etd;
            int i = 0;
            int j = 0;

            FRouteInfo = new TSw_EcdisImportAndExportRouteInfoType();
            FWaypoints = new List<TSw_EcdisImportAndExportLegWaypointType>();

            List<string> fileLines = null;

            _formatProvider.NumberDecimalSeparator = ".";

            // Commented out, cause it throws FileNotFoundException for some reason, maybe access rights or so?
            //if (!File.Exists(filePath))
            //{
            //    throw new Exception("Furuno route file " + filePath + " not found!");
            //}

            try
            {
                FRouteInfo.routeId = Path.GetFileNameWithoutExtension(filePath);

                fileLines = new List<string>(File.ReadAllLines(filePath));

                FRouteInfo.wpCount = fileLines.Count - 5;

                // Discard first three lines
                currentLine = fileLines[3];

                etd = GetLongDateTime(ref currentLine); // May be 0 (DateTime.MinValue)

                if (etd > DateTime.MinValue)
                {
                    FRouteInfo.etd = etd;
                }

                i = 5;
                j = 0;

                // Waypoint and leg information, starting at line 6
                while (i < fileLines.Count)
                {
                    currentLine = fileLines[i];
                    TSw_EcdisImportAndExportLegWaypointType waypoint = new TSw_EcdisImportAndExportLegWaypointType();

                    // Get waypoint name
                    waypointName = GetStringBeforeTab(ref currentLine);

                    // Find next tab and get lat string
                    lat = FurunoLatOrLongStrToDouble(GetStringBeforeTab(ref currentLine));

                    // Find next tab and get lon substring
                    lon = FurunoLatOrLongStrToDouble(GetStringBeforeTab(ref currentLine));

                    // Find next tab and get legtype substring
                    legType = GetStringBeforeTab(ref currentLine);

                    // Turn radius
                    turnRadius = Convert.ToDouble(GetStringBeforeTab(ref currentLine), _formatProvider);

                    // Read channel limit (but we do not use it)
                    GetStringBeforeTab(ref currentLine);

                    // Speed
                    speed = Convert.ToDouble(GetStringBeforeTab(ref currentLine), _formatProvider);

                    // Assign values to the waypoint object
                    waypoint.waypointName = waypointName;
                    waypoint.primaryNo = j + 1;
                    waypoint.secondaryNo = 0;
                    waypoint.wpType = TSw_EcdisImportAndExportWaypointType.user;
                    waypoint.latitude = lat;
                    waypoint.longitude = lon;
                    if (legType.Equals(FRF_GC))
                    {
                        waypoint.followingLegType = TSw_EcdisImportAndExportLegType.gc;
                    }
                    else
                    {
                        waypoint.followingLegType = TSw_EcdisImportAndExportLegType.rl;
                    }
                    waypoint.turnRadius = turnRadius;
                    waypoint.speed = speed;
                    waypoint.etd = DateTime.MinValue;
                    waypoint.followingLegDistanceInNauticalMile = -1;
                    waypoint.isOptimizerWp = false;

                    FWaypoints.Add(waypoint);

                    i++;
                    j++;
                }

                if (FWaypoints.Count < 1)
                {
                    throw new Exception("No waypoints were read.");
                }
            }
            catch (Exception exInner)
            {
                Exception ex = new Exception("Error occurred when reading Furuno file " + filePath + ". ", exInner);
                throw ex;
            }
            
            returnValue = true;
            return returnValue; 
        }



        public bool WriteRouteFile(string filePath) { throw new NotImplementedException(); return true; }

        public TSw_EcdisImportAndExportRouteInfoType GetRouteInfo() { throw new NotImplementedException();  return new TSw_EcdisImportAndExportRouteInfoType(); }
        public TSw_EcdisImportAndExportLegWaypointType GetWaypoint(int waypointIndex) { throw new NotImplementedException();  return new TSw_EcdisImportAndExportLegWaypointType(); }
        public void SetRouteInfo(TSw_EcdisImportAndExportRouteInfoType routeInfo) { throw new NotImplementedException(); }
        public void SetWaypoint(int waypointIndex, TSw_EcdisImportAndExportLegWaypointType waypointType) { throw new NotImplementedException(); }

        private string GetStringBeforeTab(ref string line)
        {
            string returnValue = string.Empty;
            
            string tab = "\t";
            int nextTabPosition = line.IndexOf(tab);
            if (nextTabPosition == -1)
            {
                returnValue = line;
            }
            else
            {
                // Extract the string before next tab position
                returnValue = line.Substring(0, nextTabPosition);
            }

            // Remove the string we just extracted
            string tmp = line.Substring(nextTabPosition + 1);
            line = tmp;

            return returnValue;
        }

        private DateTime GetLongDateTime(ref string line)
        {
            DateTime returnValue = DateTime.MinValue;
            
            int year = Convert.ToInt32(GetStringBeforeTab(ref line));

            if (year > 0)
            {
                int month = Convert.ToInt32(GetStringBeforeTab(ref line));
                int day = Convert.ToInt32(GetStringBeforeTab(ref line));
                int hour = Convert.ToInt32(GetStringBeforeTab(ref line));
                int minute = Convert.ToInt32(GetStringBeforeTab(ref line));
                returnValue = new DateTime(year, month, day, hour, minute, 0);
            }

            return returnValue;
        }

        
        /// <summary>
        /// Converts a lat string on format 54 12.345 N 
        /// (or long string on format 017 45.000 E) to a floating point value
        /// </summary>
        /// <param name="latOrLong">Latitude or Longitude as string</param>
        /// <returns>Latitude or Longitude as double</returns>
        private double FurunoLatOrLongStrToDouble(string latOrLong)
        {
            double returnValue = 0.0;
            double sign = 0.0;

            if (latOrLong.Length > 0)
            {
                latOrLong = latOrLong.Trim();

                // Split string "54 12.345 N" into following three strings: "54", "12.345", "N"
                string[] separator = new string[1] { " " };
                string[] splittedString = latOrLong.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                int degrees = Convert.ToInt32(splittedString[0].Trim());

                double minutes = Convert.ToDouble(splittedString[1].Trim().Replace(',', '.'), _formatProvider);

                char lastCharacter = Convert.ToChar(splittedString[2].Trim());
                if (lastCharacter == 'N' || lastCharacter == 'E')
                {
                    sign = 1.0;
                }
                else
                {
                    sign = -1.0;
                }

                returnValue = sign * (degrees + (minutes / 60.0));
            }

            return returnValue;
        }
 
    }
}
