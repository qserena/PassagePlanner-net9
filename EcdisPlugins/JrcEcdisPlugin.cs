using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.IO;
using EcdisLayer;
using System.Globalization;
using System.Runtime.Serialization.Formatters.Binary;

namespace MultiEcdisPlugin
{
    [Export(typeof(EcdisLayer.IEcdisPlugin))]
    [ExportMetadata("EcdisName", "JRC")]
    public class JrcEcdisPlugin : EcdisLayer.IEcdisPlugin
    {
        private class TFileManagement
        {
            public TFileManagement()
            {
                Spare1 = new char[3];
                FileComment = new char[64];
                Spare2 = new char[30];
                OriginalFile = new char[16];
            }
            
            public uint FileSize;
            public uint WpNo;
            public uint PosDataNo;
            public char GeodeticDatum;
            public char[] Spare1; // array[0..2] of char;
            public char[] FileComment;  // array[0..63] of char;
            public char FileAttr;
            public char Safety;
            public char[] Spare2; // array[0..29] of char;
            public char[] OriginalFile;  // array[0..15] of char;
        
        }

        private class TDataRecord
        {
            public TDataRecord()
            {
                Spare1 = new char[2];
                WpName = new char[32];
                Spare3 = new char[12];
                Spare4 = new char[4];
            }

            public uint WpNo;
            public char Safety;
            public char[] Spare1; // array[0..1] of char;
            public char AltCheck;
            public char[] WpName; // array[0..31] of char;
            public double Lat;
            public double Long;
            public uint PosDataIndex;
            public uint PosNo;
            public double LeftXte;
            public double RightXte;
            public double ArrCircle;
            public double PlannedSpeed;
            public double Rot;
            public double TurnRadius;
            public char NavMode;
            public char Spare2;
            public Int16 TimeZone;
            public char[] Spare3; // array[0..11] of char;
            public double Course;
            public double Distance;
            public double Ttg;
            public uint Eta;
            public char[] Spare4; // array[0..3] of char;
            public double WopLat;
            public double WopLong;
            public double WopLeftLat;
            public double WopLeftLong;
            public double WopRightLat;
            public double WopRightLong;
        }

        public TSw_EcdisImportAndExportRouteInfoType FRouteInfo { get; set; }

        public List<TSw_EcdisImportAndExportLegWaypointType> FWaypoints { get; set; }

        public bool CanImport() { return true; }

        public bool CanExport() { return false; }

        public string GetMaker() { return "JRC"; }

        public int GetVersion() { return 100; } // 00.00.01.00 

        public List<string> GetSupportedFileTypes() { return new List<string>() { ".rtn", ".rta" }; }

        public bool IsSupportedFileType(string extension) 
        {
            return (extension.ToLower().EndsWith("rtn") || extension.ToLower().EndsWith("rta")); 
        }


        public bool ReadRouteFile(string filePath) 
        {
            bool returnValue = false;
            TFileManagement fileMgmt = new TFileManagement();
            TDataRecord[] wps = null;

            FRouteInfo = new TSw_EcdisImportAndExportRouteInfoType();
            FWaypoints = new List<TSw_EcdisImportAndExportLegWaypointType>();

            // Commented out, cause it throws FileNotFoundException for some reason, maybe access rights or so?
            //if (!File.Exists(filePath))
            //{
            //    throw new Exception("JRC route file " + filePath + " not found!");
            //}

            try
            {
                using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open), Encoding.UTF8))
                {
                    // File management record (128 byte)
                    fileMgmt.FileSize = reader.ReadUInt32();
                    fileMgmt.WpNo = reader.ReadUInt32();
                    fileMgmt.PosDataNo = reader.ReadUInt32();
                    fileMgmt.GeodeticDatum = reader.ReadChar();
                    fileMgmt.Spare1 = reader.ReadChars(3);
                    fileMgmt.FileComment = reader.ReadChars(64);
                    fileMgmt.FileAttr = reader.ReadChar();
                    fileMgmt.Safety = reader.ReadChar();
                    fileMgmt.Spare2 = reader.ReadChars(30);
                    fileMgmt.OriginalFile = reader.ReadChars(16);

                    // Data records (1 waypoint = 208 byte)
                    wps = new TDataRecord[fileMgmt.WpNo];
                    for (int i = 0; i < fileMgmt.WpNo; i++)
                    {
                        wps[i] = new TDataRecord();
                        wps[i].WpNo = reader.ReadUInt32();
                        wps[i].Safety = reader.ReadChar();
                        wps[i].Spare1 = reader.ReadChars(2);
                        wps[i].AltCheck = reader.ReadChar();
                        wps[i].WpName = reader.ReadChars(32);
                        wps[i].Lat = reader.ReadDouble();
                        wps[i].Long = reader.ReadDouble();
                        wps[i].PosDataIndex = reader.ReadUInt32();
                        wps[i].PosNo = reader.ReadUInt32();

                        // Waypoint setting data
                        wps[i].LeftXte = reader.ReadDouble();
                        wps[i].RightXte = reader.ReadDouble();
                        wps[i].ArrCircle = reader.ReadDouble();
                        wps[i].PlannedSpeed = reader.ReadDouble();
                        wps[i].Rot = reader.ReadDouble();
                        wps[i].TurnRadius = reader.ReadDouble();
                        wps[i].NavMode = reader.ReadChar();
                        wps[i].Spare2 = reader.ReadChar();
                        wps[i].TimeZone = reader.ReadInt16();
                        wps[i].Spare3 = reader.ReadChars(12);

                        // Waypoint calculation data
                        wps[i].Course = reader.ReadDouble();
                        wps[i].Distance = reader.ReadDouble();
                        wps[i].Ttg = reader.ReadDouble();
                        wps[i].Eta = reader.ReadUInt32();
                        wps[i].Spare4 = reader.ReadChars(4);
                        wps[i].WopLat = reader.ReadDouble();
                        wps[i].WopLong = reader.ReadDouble();
                        wps[i].WopLeftLat = reader.ReadDouble();
                        wps[i].WopLeftLong = reader.ReadDouble();
                        wps[i].WopRightLat = reader.ReadDouble();
                        wps[i].WopRightLong = reader.ReadDouble();
                    }

                    reader.Close(); // Not needed?
                }
            }
            catch (FileNotFoundException ioEx)
            {
                throw ioEx;
            }

            try
            {
                FRouteInfo.routeId = Path.GetFileNameWithoutExtension(filePath);
                FRouteInfo.wpCount = Convert.ToInt32(fileMgmt.WpNo);

                if (wps != null)
                {
                    int i = 0;

                    foreach (TDataRecord wp in wps)
                    {
                        TSw_EcdisImportAndExportLegWaypointType waypoint = new TSw_EcdisImportAndExportLegWaypointType();

                        if (i > 0)
                        {
                            if (wp.NavMode == '\0')
                            {
                                FWaypoints[i - 1].followingLegType = TSw_EcdisImportAndExportLegType.gc;
                            }
                            else
                            {
                                FWaypoints[i - 1].followingLegType = TSw_EcdisImportAndExportLegType.rl;
                            }
                        }

                        // waypointName: Convert char[] to string (string will still include null characters here)
                        string waypointNameWithNullCharacters = new string(wp.WpName);

                        // Cut off string at first occurence of null character
                        waypoint.waypointName = waypointNameWithNullCharacters.Substring(0, waypointNameWithNullCharacters.IndexOf('\0'));

                        waypoint.primaryNo = Convert.ToInt32(wp.WpNo) + 1;
                        waypoint.secondaryNo = 0;
                        waypoint.wpType = TSw_EcdisImportAndExportWaypointType.user;
                        waypoint.latitude = wp.Lat;
                        waypoint.longitude = wp.Long; 
                        waypoint.turnRadius = wp.TurnRadius;
                        waypoint.turnRate = wp.Rot;
                        waypoint.speed = wp.PlannedSpeed;
                        waypoint.etd = DateTime.MinValue;
                        waypoint.followingLegDistanceInNauticalMile = -1;
                        waypoint.isOptimizerWp = false;

                        FWaypoints.Add(waypoint);

                        i++;
                    }   
                }

                if (FWaypoints.Count < 1)
                {
                    throw new Exception("No waypoints were read.");
                }
            } 
            catch (Exception exInner)
            {
                Exception ex = new Exception("Error occurred when reading JRC file " + filePath + ". ", exInner);
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

    }

    
}
