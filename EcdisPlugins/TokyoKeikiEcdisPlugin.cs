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
    // This plugin is made from the following example file, received from Mr Siddharaj Pathare, Singapore by email 2013-12-05: 
    //
    // Route Name:,Basrah-Huizhou,,,,,,,,,,,
    // Way Point,Position,,"Radius
    // (m)","Reach
    // (L)","ROT
    // (°/min)","XTD
    // (m)","SPD
    // (kn)",RL/GC,"Leg
    // (°)",Distance (NM),,ETA
    // ID,LAT,LON,,,,,,,,To WPT,TOTAL,
    // WPT1,29-39.900'N,048-49.750'E,,,,50.0,5.0,RL,180.0,0.0,0.0,03:00:00 11/17/2013
    // WPT2,29-39.500'N,048-49.750'E,2324,2.0L,4.6,50.0,6.0,RL,147.1,0.4,0.4,03:04:48 11/17/2013
    // WPT3,29-33.900'N,048-53.900'E,2324,2.0L,4.6,50.0,6.0,RL,164.7,6.1,6.5,04:05:50 11/17/2013
    // WPT4,29-31.200'N,048-54.750'E,2324,2.0L,4.6,50.0,6.0,RL,175.0,2.9,9.4,04:34:37 11/17/2013
    // ...
    // ... (the rest of the waypoints are left out here)

    [Export(typeof(EcdisLayer.IEcdisPlugin))]
    [ExportMetadata("EcdisName", "Tokyo Keiki")]
    public class TokyoKeikiEcdisPlugin : EcdisLayer.IEcdisPlugin
    {
        const string FRF_ROUTENAME = "Route Name:";
        const string FRF_RHUMBLINE = "RL";
        const string FRF_GREATCIRCLE = "GC";

        public TSw_EcdisImportAndExportRouteInfoType FRouteInfo { get; set; }
        public List<TSw_EcdisImportAndExportLegWaypointType> FWaypoints { get; set; }

        public bool CanImport() { return true; }
        public bool CanExport() { return false; }
        public string GetMaker() { return "Tokyo Keiki"; }
        public int GetVersion() { return 00000; } // 00.00.00.00 (unknown version)
        public List<string> GetSupportedFileTypes() { return new List<string>() { ".csv" }; }
        public bool IsSupportedFileType(string extension) { return extension.ToLower().EndsWith("csv"); }

        NumberFormatInfo _formatProvider = System.Globalization.NumberFormatInfo.InvariantInfo;

        NumberStyles _styles = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign; 

        public bool ReadRouteFile(string filePath) 
        {
           
            DateTime etd;

            int i = 0;

            FRouteInfo = new TSw_EcdisImportAndExportRouteInfoType();
            FWaypoints = new List<TSw_EcdisImportAndExportLegWaypointType>();

            List<string> fileLines = null;

            // Commented out, cause it throws FileNotFoundException for some reason, maybe access rights or so?
            //if (!File.Exists(filePath))
            //{
            //    throw new Exception("Tokyo Keiki route file " + filePath + " not found!");
            //}

            try
            {
                fileLines = new List<string>(File.ReadAllLines(filePath));

                bool routeNameFound = false;
                bool etdFound = false;

                foreach (string currentLine in fileLines)
                {
                    if (currentLine.Length > 0 && currentLine.Contains(","))
                    {
                        string[] splittedLine = currentLine.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                        // Route name
                        if (!routeNameFound && splittedLine.Length > 1 && splittedLine[0].Trim() == FRF_ROUTENAME)
                        {
                            FRouteInfo.routeId = splittedLine[1].Trim();
                            routeNameFound = true;
                        }

                        // Check if this line represents a waypoint
                        if (IsWaypoint(ref splittedLine))
                        {
                            bool latitudeFound = false;
                            bool longitudeFound = false;
                            double latitude = 0.0;
                            double longitude = 0.0;


                            TSw_EcdisImportAndExportLegWaypointType waypoint = new TSw_EcdisImportAndExportLegWaypointType();


                            if (splittedLine.Length < 3)
                            {
                                // Actually the length shall be much more than 3, but...
                                return false;
                            }

                            waypoint.waypointName = splittedLine[0];

                            latitudeFound = TokyoKeikiLatOrLongIsFound(splittedLine[1], out latitude);

                            longitudeFound = TokyoKeikiLatOrLongIsFound(splittedLine[2], out longitude);

                            if (!(latitudeFound && longitudeFound))
                            {
                                // Something is wrong if we didn't found both latitude and longitude here
                                return false;
                            }

                            waypoint.latitude = latitude;
                            waypoint.longitude = longitude;

                            int legTypeIndex = -1;

                            int j = 0;
                            foreach (string element in splittedLine)
                            {
                                if (element.Trim().Equals(FRF_RHUMBLINE) || element.Trim().Equals(FRF_GREATCIRCLE))
                                {
                                    // Leg type found

                                    if (element.Trim().Equals(FRF_GREATCIRCLE))
                                    {
                                        waypoint.followingLegType = TSw_EcdisImportAndExportLegType.gc;
                                    }
                                    else
                                    {
                                        waypoint.followingLegType = TSw_EcdisImportAndExportLegType.rl;
                                    }

                                    legTypeIndex = j;
                                }

                                j++;
                            }

                            // Speed shall be the value just before Leg type!
                            if (legTypeIndex > -1)
                            {
                                int speedIndex = legTypeIndex - 1;
                                double speed = 0.0;
                                Double.TryParse(splittedLine[speedIndex], _styles, _formatProvider, out speed);
                                waypoint.speed = speed;
                            }

                            // Assign values to the waypoint object
                            waypoint.primaryNo = i + 1;
                            waypoint.secondaryNo = 0;
                            waypoint.wpType = TSw_EcdisImportAndExportWaypointType.user;


                            // Set Etd as the Eta of the first waypoint.
                            if (!etdFound)
                            {
                                etd = GetDateTime(splittedLine[splittedLine.Length - 1]);
                                if (etd > DateTime.MinValue)
                                {
                                    waypoint.etd = etd;
                                    FRouteInfo.etd = etd;
                                    etdFound = true;
                                }
                            }
                            else
                            {
                                waypoint.etd = DateTime.MinValue;
                            }

                            waypoint.followingLegDistanceInNauticalMile = -1;
                            waypoint.isOptimizerWp = false;

                            FWaypoints.Add(waypoint);

                            i++;
                        }
                    }
                }

                if (FWaypoints.Count < 1)
                {
                    throw new Exception("No waypoints were read.");
                }
            }
            catch (Exception exInner)
            {
                Exception ex = new Exception("Error occurred when reading Tokyo Keiki file " + filePath + ". ", exInner);
                throw ex;
            }
            
            return true; 
        }

        /// <summary>
        /// We assume that this line represents a waypoint if it contains latitude or longitude 
        /// </summary>
        /// <returns>true, if this line represents a waypoint</returns>
        private bool IsWaypoint(ref string[] splittedLine)
        {
            double latitudeOrLongitudeValue = 0.0;

            foreach (string element in splittedLine)
            {
                if (TokyoKeikiLatOrLongIsFound(element, out latitudeOrLongitudeValue))
                {
                    return true;
                }
            }

            return false;
        }



        public bool WriteRouteFile(string filePath) { throw new NotImplementedException(); return true; }

        public TSw_EcdisImportAndExportRouteInfoType GetRouteInfo() { throw new NotImplementedException();  return new TSw_EcdisImportAndExportRouteInfoType(); }
        public TSw_EcdisImportAndExportLegWaypointType GetWaypoint(int waypointIndex) { throw new NotImplementedException();  return new TSw_EcdisImportAndExportLegWaypointType(); }
        public void SetRouteInfo(TSw_EcdisImportAndExportRouteInfoType routeInfo) { throw new NotImplementedException(); }
        public void SetWaypoint(int waypointIndex, TSw_EcdisImportAndExportLegWaypointType waypointType) { throw new NotImplementedException(); }


        private DateTime GetDateTime(string line)
        {
            DateTime result = DateTime.MinValue;
            DateTime.TryParse(line, out result);
           
            return result;
        }

        

        /// <summary>
        /// Converts a lat string on format 54-12.345'N 
        /// (or long string on format 017-45.000'E) to a floating point value
        /// 
        /// This method is used both for find out if the given string really is a lat or long,
        /// AND for converting the lat or long string to its corresponding double value.
        /// </summary>
        /// <param name="latOrLong">Latitude or Longitude as string</param>
        /// <param name="latitudeOrLongitude">Latitude or Longitude value</param>
        /// <returns>true if Latitude or Longitude is found</returns>
        private bool TokyoKeikiLatOrLongIsFound(string latOrLong, out double latitudeOrLongitude)
        {
            latitudeOrLongitude = 0.0;
            bool parseSuccess = false;
            double sign = 0.0;

            if (latOrLong.Length < 1 || !latOrLong.Contains("-") || !latOrLong.Contains("'"))
            {
                return false;
            }

            latOrLong = latOrLong.Trim();

            // Split string "54-12.345'N" into following strings: "54", "12.345'N"
            string[] separator1 = new string[1] { "-" };
            string[] splittedString1 = latOrLong.Split(separator1, StringSplitOptions.RemoveEmptyEntries);

            if (splittedString1.Length != 2)
            {
                return false;
            }

            int degrees;
            parseSuccess = Int32.TryParse(splittedString1[0], _styles, _formatProvider, out degrees);

            if (parseSuccess == false)
            {
                return false;
            }

            // Split string "12.345'N" into following strings: "12.345", "N"
            string[] separator2 = new string[1] { "'" };
            string[] splittedString2 = splittedString1[1].Split(separator2, StringSplitOptions.RemoveEmptyEntries);

            if (splittedString2.Length != 2)
            {
                return false;
            }

            double minutes;
            parseSuccess = Double.TryParse(splittedString2[0], _styles, _formatProvider, out minutes);

            if (parseSuccess == false)
            {
                return false;
            }

            char lastCharacter;
            parseSuccess = Char.TryParse(splittedString2[1], out lastCharacter);

            if (parseSuccess == false)
            {
                return false;
            }

            if (lastCharacter == 'N' || lastCharacter == 'E')
            {
                sign = 1.0;
            }
            else if (lastCharacter == 'S' || lastCharacter == 'W')
            {
                sign = -1.0;
            }
            else
            {
                return false;
            }

            latitudeOrLongitude = sign * (degrees + (minutes / 60.0));
     
            return true;
        }
    }
}
