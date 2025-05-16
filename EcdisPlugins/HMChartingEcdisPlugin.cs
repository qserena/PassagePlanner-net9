using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.IO;
using EcdisLayer;
using System.Globalization;
using System.Data;

namespace MultiEcdisPlugin
{
    // HMCharting, also known as Jeppesen

    [Export(typeof(EcdisLayer.IEcdisPlugin))]
    [ExportMetadata("EcdisName", "Jeppesen")]
    public class HMChartingEcdisPlugin : EcdisLayer.IEcdisPlugin
    {
        public TSw_EcdisImportAndExportRouteInfoType FRouteInfo { get; set; }
        public List<TSw_EcdisImportAndExportLegWaypointType> FWaypoints { get; set; }

        public bool CanImport() { return true; }
        public bool CanExport() { return false; }
        public string GetMaker() { return "Jeppesen"; }
        public int GetVersion() { return 10000; } // 00.01.00.00
        public List<string> GetSupportedFileTypes() { return new List<string>() { ".route" }; }
        public bool IsSupportedFileType(string extension) { return extension.ToLower().EndsWith("route"); }


        public bool ReadRouteFile(string filePath)
        {
            bool departureTagFound = false;

            FRouteInfo = new TSw_EcdisImportAndExportRouteInfoType();
            FWaypoints = new List<TSw_EcdisImportAndExportLegWaypointType>();

            List<string> fileLines = null;

            // Commented out, cause it throws FileNotFoundException for some reason, maybe access rights or so?
            //if (!File.Exists(filePath))
            //{
            //    throw new Exception("HMCharting route file " + filePath + " not found!");
            //}

            try
            {
                fileLines = new List<string>(File.ReadAllLines(filePath));

                NumberStyles styles = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign; 

                foreach (string line in fileLines)
                {
                    if (line.Contains("["))
                    {
                        FRouteInfo.routeId = line.Replace("[", string.Empty).Replace("]", string.Empty).Trim();
                    }
                    if (line.Contains("Departure="))
                    {
                        string departureString = line.Replace("Departure=", string.Empty).Trim();
                        if (departureString.Length > 0)
                        {
                            double departure = double.Parse(departureString, styles, System.Globalization.NumberFormatInfo.InvariantInfo);
                            FRouteInfo.etd = DateTime.FromOADate(departure);
                        }
                        departureTagFound = true;
                    }
                }

                // Get waypoints
                int j = 1;
                for (int i = 0; i < fileLines.Count - 2; i++)
                {
                    // Find Lon and Lat for this j
                    if (fileLines[i].Contains("Lon" + j.ToString() + "=") && fileLines[i + 1].Contains("Lat" + j.ToString() + "="))
                    {
                        TSw_EcdisImportAndExportLegWaypointType waypoint = new TSw_EcdisImportAndExportLegWaypointType();
                        waypoint.primaryNo = j;

                        string longitudeString = fileLines[i].Replace("Lon" + j.ToString() + "=", string.Empty).Trim();
                        waypoint.longitude = double.Parse(longitudeString, styles, System.Globalization.NumberFormatInfo.InvariantInfo);

                        string latitudeString = fileLines[i + 1].Replace("Lat" + j.ToString() + "=", string.Empty).Trim();
                        waypoint.latitude = double.Parse(latitudeString, styles, System.Globalization.NumberFormatInfo.InvariantInfo);

                        // Look some rows/lines further down, to find: (Waypoint) Name, Speed and Leg Type
                        for(int k = i + 2; k < i + 14; k++)
                        {
                            string nameTag = "Name" + j.ToString() + "=";
                            string speedTag = "Speed" + j.ToString() + "=";
                            string legTypeTag = "RhumbLine" + j.ToString() + "=";

                            if (fileLines[k].Contains(nameTag))
                            {
                                string nameString = fileLines[k].Replace(nameTag, string.Empty).Trim();
                                if (nameString.Length > 0)
                                {
                                    waypoint.waypointName = nameString;
                                }
                            }
                            else if (fileLines[k].Contains(speedTag))
                            {
                                string speedString = fileLines[k].Replace(speedTag, string.Empty).Trim();
                                if (speedString.Length > 0)
                                {
                                    waypoint.speed = double.Parse(speedString, styles, System.Globalization.NumberFormatInfo.InvariantInfo);
                                }
                            }
                            else if (fileLines[k].Contains(legTypeTag))
                            {
                                string legTypeString = fileLines[k].Replace(legTypeTag, string.Empty).Trim();
                                if (legTypeString.Length > 0 && legTypeString == "0")
                                {
                                    waypoint.followingLegType = TSw_EcdisImportAndExportLegType.gc;
                                }
                                else
                                {
                                    waypoint.followingLegType = TSw_EcdisImportAndExportLegType.rl;
                                }
                            }
                        }

                        waypoint.wpType = TSw_EcdisImportAndExportWaypointType.user;
                        waypoint.isOptimizerWp = false;
                        FWaypoints.Add(waypoint);
                        FRouteInfo.wpCount = j;

                        j++;
                    }
                }

                if (FWaypoints.Count > 1 && departureTagFound)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception exInner)
            {
                Exception ex = new Exception("Error occurred when reading Jeppesen file " + filePath + ". ", exInner);
                throw ex;
            }
        }

        private void GetWaypoints()
        {
            throw new NotImplementedException();
        }

        public bool WriteRouteFile(string filePath) { throw new NotImplementedException(); return false; }
        public TSw_EcdisImportAndExportRouteInfoType GetRouteInfo() { throw new NotImplementedException();  return new TSw_EcdisImportAndExportRouteInfoType(); }
        public TSw_EcdisImportAndExportLegWaypointType GetWaypoint(int waypointIndex) { throw new NotImplementedException();  return new TSw_EcdisImportAndExportLegWaypointType(); }
        public void SetRouteInfo(TSw_EcdisImportAndExportRouteInfoType routeInfo) { throw new NotImplementedException(); }
        public void SetWaypoint(int waypointIndex, TSw_EcdisImportAndExportLegWaypointType waypointType) { throw new NotImplementedException(); }

    }
}
