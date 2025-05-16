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
    [ExportMetadata("EcdisName", "SAM Electronics")]
    public class SamEcdisPlugin : EcdisLayer.IEcdisPlugin
    {
        // Implementation made to be able to import follwing example file
        //
        //#;CatalogName;import;
        //#;MapName;HKI-KAS-REV-STO;
        //#;MapNumber;1100;
        //@;No.;Latitude    Longitude;ETA;PlSp;Rad;TTG;TrkDst;Remark;
        //0001;60:09.679 N 024:57.564 E;18.03.05 09:02; 1.0;0.40;      ;  0.00;AVGÅNG HELSINGFORS;
        //0005;60:08.381 N 024:59.535 E;18.03.05 09:26;12.0;0.30;  3:59;  2.58;G-SVÄRD           ;
        //0008;60:06.535 N 024:58.804 E;18.03.05 09:33;16.0;1.00;  4:07;  4.48;GRÅHARA           ;
        //0010;59:59.442 N 024:55.763 E;18.03.05 09:56;20.0;2.00; 15:41; 11.75;NYGRUND           ;


        public TSw_EcdisImportAndExportRouteInfoType FRouteInfo { get; set; }
        public List<TSw_EcdisImportAndExportLegWaypointType> FWaypoints { get; set; }

        public bool CanImport() { return true; }
        public bool CanExport() { return false; }
        public string GetMaker() { return "SAM Electronics"; }
        public int GetVersion() { return 00000; } // Unknown version
        public List<string> GetSupportedFileTypes() { return new List<string>() { ".txt" }; }
        public bool IsSupportedFileType(string extension) { return extension.ToLower().EndsWith("txt"); }

        NumberFormatInfo _formatProvider = new NumberFormatInfo();
        //NumberFormatInfo _formatProvider = System.Globalization.NumberFormatInfo.InvariantInfo;
        
        public bool ReadRouteFile(string filePath) 
        {
            bool returnValue = false;
            int currentLineNo = 0;
            int waypointCounter = 0;
            string latitudeString = string.Empty;
            string longitudeString = string.Empty;

            DateTime etd;

            FRouteInfo = new TSw_EcdisImportAndExportRouteInfoType();
            FWaypoints = new List<TSw_EcdisImportAndExportLegWaypointType>();

            List<string> fileLines = null;

            _formatProvider.NumberDecimalSeparator = ".";

            // Commented out, cause it throws FileNotFoundException for some reason, maybe access rights or so?
            //if (!File.Exists(filePath))
            //{
            //    throw new Exception("Sam Electronics route file " + filePath + " not found!");
            //}

            try
            {
                FRouteInfo.routeId = Path.GetFileNameWithoutExtension(filePath);

                fileLines = new List<string>(File.ReadAllLines(filePath, Encoding.UTF7)); // UTF7 used to manage characters å, ä and ö. 

                while (currentLineNo < fileLines.Count - 1 && !fileLines[currentLineNo].StartsWith("@"))
                {
                    currentLineNo++;
                }
                if (currentLineNo > fileLines.Count)
                {
                    throw new Exception("Character '@' not found in file " + filePath);
                }

            
                // The headers are in the same row/line as the '@' character (which we just found)
                // Put headers in a list of strings (split with ';' character). 
                List<string> headers = fileLines[currentLineNo].Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList<string>(); 


                int positionColumnIndex = -1;
                int etaColumnIndex = -1;
                int plannedSpeedColumnIndex = -1;
                int turnRadiusColumnIndex = -1;
                int remarksColumnIndex = -1;

                // Find out the column position/index of each header.
                // In this way we manage to read the file even if there would be additional headers stuck in between,
                // compared with the example file, even if the headers probably will be in the same order as in the
                // example file, which is: No.;Latitude    Longitude;ETA;PlSp;Rad;TTG;TrkDst;Remark;
                foreach (string header in headers)
                {
                    if (header.Trim().ToLower().StartsWith("latitude"))
                    {
                        // We subtract 1 because header line starts with '@' character
                        positionColumnIndex = headers.IndexOf(header) - 1;
                    }

                    if (header.Trim().ToLower().StartsWith("eta"))
                    {
                        etaColumnIndex = headers.IndexOf(header) - 1;
                    }

                    if (header.Trim().ToLower().StartsWith("plsp"))
                    {
                        plannedSpeedColumnIndex = headers.IndexOf(header) - 1;
                    }

                    if (header.Trim().ToLower().StartsWith("rad"))
                    {
                        turnRadiusColumnIndex = headers.IndexOf(header) - 1;
                    }

                    if (header.Trim().ToLower().StartsWith("remark"))
                    {
                        remarksColumnIndex = headers.IndexOf(header) - 1;
                    }
                }

                //etd = GetLongDateTime(ref currentLine); // May be 0 (DateTime.MinValue)

                //if (etd > DateTime.MinValue)
                //{
                //    FRouteInfo.etd = etd;
                //}

                bool etaForFirstWaypointFound = false;

                // Read the waypoints
                while (currentLineNo < fileLines.Count - 1)
                {
                    // Go to next line
                    currentLineNo++;

                    // Put current line in a list of strings (split with ';' character). 
                    string[] currentLineArray = fileLines[currentLineNo].Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    List<string> currentLine = currentLineArray.ToList<string>();

                    TSw_EcdisImportAndExportLegWaypointType waypoint = new TSw_EcdisImportAndExportLegWaypointType();

                    // Latitude and Longitude
                    if (positionColumnIndex > 0)
                    {
                        string latAndLongString = currentLine[positionColumnIndex];

                        // Separate lat from long in the string
                        latitudeString = latAndLongString.Substring(0, 11).Trim();
                        longitudeString = latAndLongString.Substring(12).Trim();

                        // Convert to double
                        waypoint.latitude = SamLatOrLongStrToDouble(latitudeString);
                        waypoint.longitude = SamLatOrLongStrToDouble(longitudeString);
                    }

                    // ETA for the first waypoint ( = departure time )
                    if (etaColumnIndex > 0 && !etaForFirstWaypointFound && FWaypoints.Count == 0)
                    {
                        string etaString = currentLine[etaColumnIndex].Trim();
                        DateTime result = DateTime.MinValue;
                        DateTime.TryParseExact(etaString, "dd.MM.yy HH:mm", _formatProvider, DateTimeStyles.AllowWhiteSpaces, out result);
                        
                        etaForFirstWaypointFound = true;
                        if (result > DateTime.MinValue)
                        {
                            FRouteInfo.etd = result;
                        }
                    }

                    // Speed
                    if (plannedSpeedColumnIndex > 0)
                    {
                        string speedString = currentLine[plannedSpeedColumnIndex].Trim().Replace(',', '.');
                        waypoint.speed = Convert.ToDouble(speedString, _formatProvider);
                    }

                    // Turn radius
                    if (turnRadiusColumnIndex > 0)
                    {
                        string turnRadiusString = currentLine[turnRadiusColumnIndex].Trim().Replace(',', '.');
                        waypoint.turnRadius = Convert.ToDouble(turnRadiusString, _formatProvider);
                    }

                    // We copy remark into waypoint name, because the example file contains this type of information. Maybe remove? 
                    if (remarksColumnIndex > 0)
                    {
                        waypoint.waypointName = currentLine[remarksColumnIndex].Trim();
                    }

                    waypoint.followingLegType = TSw_EcdisImportAndExportLegType.rl; // Sam Electronics does not provide leg type?
                    waypoint.wpType = TSw_EcdisImportAndExportWaypointType.user;
                    waypoint.etd = DateTime.MinValue;
                    waypoint.followingLegDistanceInNauticalMile = -1;
                    waypoint.isOptimizerWp = false;

                    FWaypoints.Add(waypoint);

                    waypointCounter++;
                }

                FRouteInfo.wpCount = waypointCounter;

                if (FWaypoints.Count < 1)
                {
                    throw new Exception("No waypoints were read.");
                }

            }
            catch (Exception exInner)
            {
                Exception ex = new Exception("Error occurred when reading SAM file " + filePath + ". ", exInner);
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

        
        /// <summary>
        /// Converts a lat string on format 54:12.345 N 
        /// (or long string on format 017:45.000 E) 
        /// to a floating point value
        /// </summary>
        /// <param name="latOrLong">Latitude or Longitude as string</param>
        /// <returns>Latitude or Longitude as double</returns>
        private double SamLatOrLongStrToDouble(string latOrLong)
        {
            double returnValue = 0.0;
            double sign = 0.0;

            if (latOrLong.Length > 0)
            {
                latOrLong = latOrLong.Trim();

                // Split string "54:12.345 N" into following two strings: "54", "12.345 N"
                string[] separator1 = new string[1] { ":" };
                string[] splittedString1 = latOrLong.Split(separator1, StringSplitOptions.RemoveEmptyEntries);

                int degrees = Convert.ToInt32(splittedString1[0].Trim());

                // Split string "12.345 N" into following two strings: "12.345", "N"
                string[] separator2 = new string[1] { " " };
                string[] splittedString2 = splittedString1[1].Split(separator2, StringSplitOptions.RemoveEmptyEntries);
                double minutes = Convert.ToDouble(splittedString2[0].Trim().Replace(',', '.'), _formatProvider);

                char lastCharacter = Convert.ToChar(splittedString2[1].Trim());
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
