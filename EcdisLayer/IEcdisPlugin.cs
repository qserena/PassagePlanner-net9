using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcdisLayer
{
    public enum TSw_EcdisImportAndExportType 
    {
        import_, 
        export_
    }

    public enum TSw_EcdisImportAndExportLegType 
    {
        rl, 
        gc
    }

    public enum TSw_EcdisImportAndExportEngineSettingUnitType 
    {
        speed, 
        rpm, 
        percentmrc
    }

    public enum TSw_EcdisImportAndExportWaypointType
    {
        user, 
        weather
    }

    public class TSw_EcdisImportAndExportLegEngineSettingType
    {
        public double value;
        public TSw_EcdisImportAndExportEngineSettingUnitType unit_;
    }

    public class TSw_EcdisImportAndExportLegWaypointType
    {
        public string waypointName;
        public int primaryNo;
        public int secondaryNo;
        public TSw_EcdisImportAndExportWaypointType wpType;
        public double turnRadius; // [NM]
        public double turnRate; // [deg/min]
        public TSw_EcdisImportAndExportLegType followingLegType;
        public double speed; // [knots]
        public double latitude; // required
        public double longitude; // required
        public DateTime etd;
        public double followingLegDistanceInNauticalMile;
        public bool isOptimizerWp;
    }

    public class TSw_EcdisImportAndExportRouteInfoType
    {
        public int wpCount;
        public string vesselName;
        public string routeId;
        public double distanceInNauticalMile;
        public DateTime etd;
        public DateTime eta;
    }

    public interface IEcdisPlugin
    {
        //
        // Copied from Sw_KongsbergRutEcdisPlugin.pas:
        //
        //function CanImport : boolean; override;
        //function CanExport : boolean; override;
        //function GetMaker : string; override;
        //function GetVersion : integer; override;
        //function GetSupportedFileTypes : TStringList; override;
        //function IsSupportedFileType(AExt : string) : boolean; override;
        //function ReadRouteFile(AFilePath : string) : boolean; override;
        //function WriteRouteFile(AFilePath : string) : boolean; override;
        //function GetRouteInfo : TSw_EcdisImportAndExportRouteInfoType; override;
        //function GetWaypoint(AIndex : integer) : TSw_EcdisImportAndExportLegWaypointType; override;
        //procedure SetRouteInfo(AInfo : TSw_EcdisImportAndExportRouteInfoType); override;
        //procedure SetWaypoint(AIndex : integer; AWp : TSw_EcdisImportAndExportLegWaypointType); override;

        TSw_EcdisImportAndExportRouteInfoType FRouteInfo { get; set; }
        List<TSw_EcdisImportAndExportLegWaypointType> FWaypoints { get; set; }
        
        bool CanImport();
        bool CanExport();
        string GetMaker();
        int GetVersion();
        List<string> GetSupportedFileTypes();
        bool IsSupportedFileType(string extension);
        bool ReadRouteFile(string filePath);
        bool WriteRouteFile(string filePath);
        TSw_EcdisImportAndExportRouteInfoType GetRouteInfo();
        TSw_EcdisImportAndExportLegWaypointType GetWaypoint(int waypointIndex);
        void SetRouteInfo(TSw_EcdisImportAndExportRouteInfoType routeInfo);
        void SetWaypoint(int waypointIndex, TSw_EcdisImportAndExportLegWaypointType waypointType);
    }

}
