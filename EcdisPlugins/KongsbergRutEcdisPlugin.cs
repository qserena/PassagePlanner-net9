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
    [ExportMetadata("EcdisName", "Kongsberg")]
    public class KongsbergRutEcdisPlugin : EcdisLayer.IEcdisPlugin
    {
        const string KRF_SAVETIME = "[savetime]";
        const string KRF_UTIME = "[updatetime]";
        const string KRF_VALTIME = "[valtime]";
        const string KRF_GEO = "[geo]";
        const string KRF_ROUTE = "[route]";
        const string KRF_WP = "[wp]";
        const string KRF_POS = "[pos]";
        const string KRF_RADIUS = "[radius]";
        const string KRF_CORRIDOR = "[corridor]";
        const string KRF_SAILMODE = "[sailmode]";
        const string KRF_SPEED = "[speed]";
        const string KRF_MESSAGE = "[message]";
        const string KRF_DANGER = "[danger]";

        public TSw_EcdisImportAndExportRouteInfoType FRouteInfo { get; set; }
        public List<TSw_EcdisImportAndExportLegWaypointType> FWaypoints { get; set; }

        public bool CanImport() { return true; }
        public bool CanExport() { return false; }
        public string GetMaker() { return "KONGSBERG"; }
        public int GetVersion() { return 1; }
        public List<string> GetSupportedFileTypes() { return new List<string>() { ".rut" }; }
        public bool IsSupportedFileType(string extension) { return extension.ToLower().EndsWith("rut"); }

        public bool ReadRouteFile(string filePath) 
        {
            bool returnValue = false;
            string tmp;
            double lat;
            double lon;
            int i = 0;
            int waypointNameCount = 0;

            FRouteInfo = new TSw_EcdisImportAndExportRouteInfoType();
            FWaypoints = new List<TSw_EcdisImportAndExportLegWaypointType>();

            List<string> fileLines = null;

            // Commented out, cause it throws FileNotFoundException for some reason, maybe access rights or so?
            //if (!File.Exists(filePath))
            //{
            //    throw new Exception("Kongsberg route file " + filePath + " not found!");
            //}

             try
            {

                fileLines = new List<string>(File.ReadAllLines(filePath));

                tmp = GetTagData(fileLines, KRF_ROUTE);
                FRouteInfo.routeId = tmp;
                if (tmp.Length == 0)
                {
                    string extension = Path.GetExtension(filePath);
                    string fileName = Path.GetFileName(filePath);
                    if (extension != null && extension.Length > 0 && fileName.EndsWith(extension))
                    {
                        int indexWhereExtensionBegins = fileName.IndexOf(extension);
                        FRouteInfo.routeId = fileName.Substring(0, indexWhereExtensionBegins);
                    }
                }

                while (true) 
                {
                    // Position
                    tmp = GetTagData(fileLines, KRF_POS);
                    if (tmp.Length == 0)
                    {
                        // No [pos] left in fileLines
                        break;
                    }
                    TSw_EcdisImportAndExportLegWaypointType waypoint = new TSw_EcdisImportAndExportLegWaypointType();
                    LatLonDegMinSecStrToDouble(tmp, out lat, out lon);
                    waypoint.latitude = lat;
                    waypoint.longitude = lon;

                    // Waypoint name
                    tmp = GetTagData(fileLines, KRF_WP);
                    if (tmp.Length > 0)
                    {
                        waypoint.waypointName = tmp;
                        waypointNameCount++;
                    }

                    // Turn radius
                    tmp = GetTagData(fileLines, KRF_RADIUS);
                    if (tmp.Length > 0)
                    {
                        waypoint.turnRadius = double.Parse(tmp, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo) / 1852.0;
                    }

                    // Leg type / Sail mode
                    tmp = GetTagData(fileLines, KRF_SAILMODE);
                    if (tmp == "0")
                    {
                        waypoint.followingLegType = TSw_EcdisImportAndExportLegType.rl;
                    }
                    else
                    {
                        waypoint.followingLegType = TSw_EcdisImportAndExportLegType.gc;
                    }

                    // Speed
                    tmp = GetTagData(fileLines, KRF_SPEED);
                    if (tmp.Length > 0)
                    {
                        waypoint.speed = double.Parse(tmp, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo);
                    }
                
                    waypoint.primaryNo = i + 1;
                    waypoint.secondaryNo = 0;
                    waypoint.wpType = TSw_EcdisImportAndExportWaypointType.user;
                    waypoint.etd = DateTime.MinValue;
                    waypoint.followingLegDistanceInNauticalMile = -1;
                    waypoint.isOptimizerWp = false;

                    FWaypoints.Add(waypoint);

                    i++;
                }

                FRouteInfo.wpCount = i;

                if (waypointNameCount < FRouteInfo.wpCount)
                {
                    // Not every waypoint has a waypoint name => Remove all waypoint names
                    // since we are not sure that the waypoint names match the waypoints
                    foreach (TSw_EcdisImportAndExportLegWaypointType wp in FWaypoints)
                    {
                        wp.waypointName = string.Empty;
                    }
                }

                 if (FWaypoints.Count < 1)
                {
                    throw new Exception("No waypoints were read.");
                }

            }
            catch (Exception exInner)
            {
                Exception ex = new Exception("Error occurred when reading Kongsberg file " + filePath + ". ", exInner);
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

        private string GetTagData(List<string> lines, string tagName)
        {
            bool found = false;
            string result = string.Empty;
            int i;
            int tagEndIndex;
            string line = string.Empty;
            for (i = 0; i < lines.Count-1; i++)
            {
                line = RemoveComment(lines[i]);
                if (line.Contains(tagName))
                {
                    found = true;
                    break;
                }
            }
  
            if (found && i < lines.Count)
            {
                tagEndIndex = line.IndexOf(']');
                if (tagEndIndex < 0)
                {
                    throw new Exception("Character ']' was not found.");
                }
                string tempString = line.Substring(tagEndIndex+1).Trim();

                // Remove " around strings
                if (tempString.Length > 1 && tempString.StartsWith("\""))
                {
                    result = tempString.Substring(1, tempString.Length - 2);
                }
                else
                {
                    result = tempString;
                }
                lines.RemoveAt(i);
                if (result.Length == 0)
                {
                    result = " ";
                }
            }
            else
            {
                result = string.Empty;
            }
            return result;
        }

        private string RemoveComment(string str)
        {
            int index = str.IndexOf('#');
            if (index >= 0)
            {
                return str.Substring(0, index).Trim();
            }
            else
            {
                return str.Trim();
            }
        }

        private void LatLonDegMinSecStrToDouble(string aString, out double lat, out double lon)
        {
            string str; 
            string latStr; 
            string lonStr;
            int i;
            str = aString.Replace('\t', ' ');
            i = str.IndexOf(' ');
            latStr = str.Substring(0, i+1).Trim();
            lonStr = str.Substring(i).Trim();
            lat = LatLonDegMinSecStrToDouble(latStr);
            lon = LatLonDegMinSecStrToDouble(lonStr);
        }

        double LatLonDegMinSecStrToDouble(string aString)
        {
            int i;
            int deg;
            int min;
            double sec;
            double sign;

            i = aString.IndexOf('.');
            deg = Convert.ToInt32(aString.Substring(0, i-4));
            min = Convert.ToInt32(aString.Substring(i-4, 2));
            sec = double.Parse(aString.Substring(i - 2), CultureInfo.InvariantCulture);
            sign = Math.Sign(deg);
            if (sign == 0 && aString.StartsWith("-")) 
            {
                sign = -1;
            }
            else if (sign == 0) 
            {
                sign = 1;
            }

            return deg+sign*(min+sec/60)/60;
        }
    }
}
