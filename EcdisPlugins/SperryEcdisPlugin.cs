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
    [Export(typeof(EcdisLayer.IEcdisPlugin))]
    [ExportMetadata("EcdisName", "Sperry Marine")]
    public class SperryEcdisPlugin : EcdisLayer.IEcdisPlugin
    {
        public TSw_EcdisImportAndExportRouteInfoType FRouteInfo { get; set; }
        public List<TSw_EcdisImportAndExportLegWaypointType> FWaypoints { get; set; }

        public bool CanImport() { return true; }
        public bool CanExport() { return false; }
        public string GetMaker() { return "Sperry Marine"; }
        public int GetVersion() { return 5030000; } // 05.03.00.00
        public List<string> GetSupportedFileTypes() { return new List<string>() { ".route" }; }
        public bool IsSupportedFileType(string extension) { return extension.ToLower().EndsWith("route"); }

        public bool ReadRouteFile(string filePath) 
        {
            int i = 0;

            FRouteInfo = new TSw_EcdisImportAndExportRouteInfoType();
            FWaypoints = new List<TSw_EcdisImportAndExportLegWaypointType>();

            // Commented out, cause it throws FileNotFoundException for some reason, maybe access rights or so?
            //if (!File.Exists(filePath))
            //{
            //    throw new Exception("Sperry Marine route file " + filePath + " not found!");
            //}

             try
            {
                DataSet dataSet = new DataSet();
                dataSet.ReadXml(filePath, XmlReadMode.ReadSchema);

                if (dataSet.Tables.Contains("Summaries") && dataSet.Tables.Contains("ControlPoints"))
                {
                    DataTable summaries = dataSet.Tables["Summaries"];

                    FRouteInfo.routeId = (string)summaries.Rows[0]["Name"];
                    DateTime departureTime = (DateTime)summaries.Rows[0]["DepartureTime"];

                    // Have to do this conversion to UTC. Otherwise "2013-06-26T11:49:54.823+00:00" in the file automatically will be
                    // "2013-06-26T13:49:54.823+00:00" here in .Net. (local time on my computer is GMT+2 hours in this example).
                    DateTime departureTimeUtc = departureTime.ToUniversalTime();

                    FRouteInfo.etd = departureTimeUtc;

                    DataTable controlPoints = dataSet.Tables["ControlPoints"];

                    foreach (DataRow controlPoint in controlPoints.Rows)
                    {
                        TSw_EcdisImportAndExportLegWaypointType waypoint = new TSw_EcdisImportAndExportLegWaypointType();

                        // Latitude and Longitude in radians
                        double latitude = (double)controlPoint["Latitude"];
                        double longitude = (double)controlPoint["Longitude"];

                        // Convert to degrees
                        waypoint.latitude = latitude * 180.0 / Math.PI;
                        waypoint.longitude = longitude * 180.0 / Math.PI;

                        double turnRadiusInMeters = (double)controlPoint["TurnRadius"];
                        waypoint.turnRadius = turnRadiusInMeters / 1852.0;

                        //double turnSpeed = (double)controlPoint["TurnSpeed"];
                      
                        double speed = (double)controlPoint["DepartingTrackSpeed"];
                        waypoint.speed = speed;

                        string legTypeString = (string)controlPoint["DepartingControlLineType"];
                        if (legTypeString == "GreatCircleLine")
                        {
                            waypoint.followingLegType = TSw_EcdisImportAndExportLegType.gc;
                        }
                        else
                        {
                            waypoint.followingLegType = TSw_EcdisImportAndExportLegType.rl;
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

                }
                else
                {
                    return false;
                }


                if (FWaypoints.Count < 1)
                {
                    throw new Exception("No waypoints were read.");
                }

            }
            catch (Exception exInner)
            {
                Exception ex = new Exception("Error occurred when reading Sperry Marine file " + filePath + ". ", exInner);
                throw ex;
            }

            return true; 
        }

        public bool WriteRouteFile(string filePath) { throw new NotImplementedException(); return false; }

        public TSw_EcdisImportAndExportRouteInfoType GetRouteInfo() { throw new NotImplementedException();  return new TSw_EcdisImportAndExportRouteInfoType(); }
        public TSw_EcdisImportAndExportLegWaypointType GetWaypoint(int waypointIndex) { throw new NotImplementedException();  return new TSw_EcdisImportAndExportLegWaypointType(); }
        public void SetRouteInfo(TSw_EcdisImportAndExportRouteInfoType routeInfo) { throw new NotImplementedException(); }
        public void SetWaypoint(int waypointIndex, TSw_EcdisImportAndExportLegWaypointType waypointType) { throw new NotImplementedException(); }

    }
}
