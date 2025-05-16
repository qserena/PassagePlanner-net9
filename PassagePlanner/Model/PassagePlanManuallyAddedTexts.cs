using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace PassagePlanner
{
    /// <summary>
    /// Container class for the manually added texts in the Passage Planner report.
    /// Will be serialized/deserialized as a member of class Route when saving and opening route files.
    /// </summary>
    public class PassagePlanManuallyAddedTexts : INotifyPropertyChanged
    {
        // gN1 = General Notes 1
        private string _gN1_DeparturePort_NameOfBerth = string.Empty;
        private string _gN1_DeparturePort_VhfChannelPortControl = string.Empty;
        private string _gN1_DeparturePort_VhfChannelPilots = string.Empty;
        private string _gN1_DeparturePort_PreDepartureNoticeFor = string.Empty;
        private string _gN1_DeparturePort_VhfChannel = string.Empty;
        private string _gN1_DeparturePort_PreDepartureNoticeForSecondRow = string.Empty;
        private string _gN1_DeparturePort_DraughtRestrictions = string.Empty;
        private string _gN1_DeparturePort_AirDraughtRestrictions = string.Empty;
        private string _gN1_DeparturePort_OtherRestrictions = string.Empty;
        private string _gN1_DeparturePort_ChangeOfPilotDuringBlaBlaBla = string.Empty;
        private string _gN1_DeparturePort_GuideToPortEntryVolPage = string.Empty;
        private string _gN1_ArrivalPort_NameOfBerth = string.Empty;
        private string _gN1_ArrivalPort_VhfChannelPortControl = string.Empty;
        private string _gN1_ArrivalPort_VhfChannelPilots = string.Empty;
        private string _gN1_ArrivalPort_PreArrivalNoticeFor = string.Empty;
        private string _gN1_ArrivalPort_VhfChannel = string.Empty;
        private string _gN1_ArrivalPort_PreArrivalNoticeForSecondRow = string.Empty;
        private string _gN1_ArrivalPort_DraughtRestrictions = string.Empty;
        private string _gN1_ArrivalPort_AirDraughtRestrictions = string.Empty;
        private string _gN1_ArrivalPort_OtherRestrictions = string.Empty;
        private string _gN1_ArrivalPort_ChangeOfPilotDuringBlaBlaBla = string.Empty;
        private string _gN1_ArrivalPort_GuideToPortEntryVolPage = string.Empty;
        private string _gN1_TidalInfoDeparture_Time1LowWater = string.Empty;
        private string _gN1_TidalInfoDeparture_Time1HighWater = string.Empty;
        private string _gN1_TidalInfoDeparture_Time1Rise = string.Empty;
        private string _gN1_TidalInfoDeparture_Time2LowWater = string.Empty;
        private string _gN1_TidalInfoDeparture_Time2HighWater = string.Empty;
        private string _gN1_TidalInfoDeparture_Time2Rise = string.Empty;
        private string _gN1_TidalInfoDeparture_StandardPort = string.Empty;
        private string _gN1_TidalInfoArrival_Time1LowWater = string.Empty;
        private string _gN1_TidalInfoArrival_Time1HighWater = string.Empty;
        private string _gN1_TidalInfoArrival_Time1Rise = string.Empty;
        private string _gN1_TidalInfoArrival_Time2LowWater = string.Empty;
        private string _gN1_TidalInfoArrival_Time2HighWater = string.Empty;
        private string _gN1_TidalInfoArrival_Time2Rise = string.Empty;
        private string _gN1_TidalInfoArrival_StandardPort = string.Empty;
        private string _gN1_GeneralDescription = string.Empty;

        // gN2 = General Notes 2
        private string _gN2_NavigationalWarnings = string.Empty;
        private string _gN2_EmergencyAnchorages = string.Empty;
        private string _gN2_TAndPNotices = string.Empty;
        private string _gN2_Weather = string.Empty;
        private string _gN2_RestrictedAreas = string.Empty;
        private string _gN2_AdditionalNotes = string.Empty;
        private string _gN2_Rcc_Station0 = string.Empty;
        private string _gN2_Rcc_Remarks0 = string.Empty;
        private string _gN2_Rcc_ChannelPhone0 = string.Empty;
        private string _gN2_Rcc_Station1 = string.Empty;
        private string _gN2_Rcc_Remarks1 = string.Empty;
        private string _gN2_Rcc_ChannelPhone1 = string.Empty;
        private string _gN2_Rcc_Station2 = string.Empty;
        private string _gN2_Rcc_Remarks2 = string.Empty;
        private string _gN2_Rcc_ChannelPhone2 = string.Empty;
        private string _gN2_Rcc_Station3 = string.Empty;
        private string _gN2_Rcc_Remarks3 = string.Empty;
        private string _gN2_Rcc_ChannelPhone3 = string.Empty;
        private string _gN2_Rcc_Station4 = string.Empty;
        private string _gN2_Rcc_Remarks4 = string.Empty;
        private string _gN2_Rcc_ChannelPhone4 = string.Empty;
        private string _gN2_AllChartsCorrectedUpTo_NoticeToMarinersNo = string.Empty;
        private string _gN2_AllChartsCorrectedUpTo_Dated = string.Empty;
        private string _gN2_IsWaterDepthAtChartDatumBlaBlaBla = string.Empty;
        private string _gN2_IfYesHaveSquatCalculationsBlaBlaBla = string.Empty;
        private string _gN2_IsOptimizedRouteFromSeawareWeather = string.Empty;
        private string _gN2_Officer0 = string.Empty;
        private string _gN2_Officer1 = string.Empty;
        private string _gN2_Officer2 = string.Empty;
        private string _gN2_Officer3 = string.Empty;
        private string _gN2_Officer4 = string.Empty;
        private string _gN2_Officer5 = string.Empty;
        private string _gN2_Master = string.Empty;

        // Additional Notes
        private string _aN_AdditionalNotes = string.Empty;


        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsDirty { get; set; }

        public PassagePlanManuallyAddedTexts()
        {
            IsDirty = false;
        }

        public string GN1_DeparturePort_NameOfBerth
        {
            get
            {
                return _gN1_DeparturePort_NameOfBerth;
            }
            set
            {
                if (_gN1_DeparturePort_NameOfBerth == value)
                {
                    return;
                }

                _gN1_DeparturePort_NameOfBerth = value;
                OnPropertyChanged("GN1_DeparturePort_NameOfBerth");
                IsDirty = true;
            }
        }

        public string GN1_DeparturePort_VhfChannelPortControl
        {
            get
            {
                return _gN1_DeparturePort_VhfChannelPortControl;
            }
            set
            {
                if (_gN1_DeparturePort_VhfChannelPortControl == value)
                {
                    return;
                }

                _gN1_DeparturePort_VhfChannelPortControl = value;
                OnPropertyChanged("GN1_DeparturePort_VhfChannelPortControl");
                IsDirty = true;
            }
        }

        public string GN1_DeparturePort_VhfChannelPilots
        {
            get
            {
                return _gN1_DeparturePort_VhfChannelPilots;
            }
            set
            {
                if (_gN1_DeparturePort_VhfChannelPilots == value)
                {
                    return;
                }

                _gN1_DeparturePort_VhfChannelPilots = value;
                OnPropertyChanged("GN1_DeparturePort_VhfChannelPilots");
                IsDirty = true;
            }
        }

        public string GN1_DeparturePort_PreDepartureNoticeFor
        {
            get
            {
                return _gN1_DeparturePort_PreDepartureNoticeFor;
            }
            set
            {
                if (_gN1_DeparturePort_PreDepartureNoticeFor == value)
                {
                    return;
                }

                _gN1_DeparturePort_PreDepartureNoticeFor = value;
                OnPropertyChanged("GN1_DeparturePort_PreDepartureNoticeFor");
                IsDirty = true;
            }
        }

        public string GN1_DeparturePort_VhfChannel
        {
            get
            {
                return _gN1_DeparturePort_VhfChannel;
            }
            set
            {
                if (_gN1_DeparturePort_VhfChannel == value)
                {
                    return;
                }

                _gN1_DeparturePort_VhfChannel = value;
                OnPropertyChanged("GN1_DeparturePort_VhfChannel");
                IsDirty = true;
            }
        }

        public string GN1_DeparturePort_PreDepartureNoticeForSecondRow
        {
            get
            {
                return _gN1_DeparturePort_PreDepartureNoticeForSecondRow;
            }
            set
            {
                if (_gN1_DeparturePort_PreDepartureNoticeForSecondRow == value)
                {
                    return;
                }

                _gN1_DeparturePort_PreDepartureNoticeForSecondRow = value;
                OnPropertyChanged("GN1_DeparturePort_PreDepartureNoticeForSecondRow");
                IsDirty = true;
            }
        }

        public string GN1_DeparturePort_DraughtRestrictions
        {
            get
            {
                return _gN1_DeparturePort_DraughtRestrictions;
            }
            set
            {
                if (_gN1_DeparturePort_DraughtRestrictions == value)
                {
                    return;
                }

                _gN1_DeparturePort_DraughtRestrictions = value;
                OnPropertyChanged("GN1_DeparturePort_DraughtRestrictions");
                IsDirty = true;
            }
        }

        public string GN1_DeparturePort_AirDraughtRestrictions
        {
            get
            {
                return _gN1_DeparturePort_AirDraughtRestrictions;
            }
            set
            {
                if (_gN1_DeparturePort_AirDraughtRestrictions == value)
                {
                    return;
                }

                _gN1_DeparturePort_AirDraughtRestrictions = value;
                OnPropertyChanged("GN1_DeparturePort_AirDraughtRestrictions");
                IsDirty = true;
            }
        }

        public string GN1_DeparturePort_OtherRestrictions
        {
            get
            {
                return _gN1_DeparturePort_OtherRestrictions;
            }
            set
            {
                if (_gN1_DeparturePort_OtherRestrictions == value)
                {
                    return;
                }

                _gN1_DeparturePort_OtherRestrictions = value;
                OnPropertyChanged("GN1_DeparturePort_OtherRestrictions");
                IsDirty = true;
            }
        }

        public string GN1_DeparturePort_ChangeOfPilotDuringBlaBlaBla
        {
            get
            {
                return _gN1_DeparturePort_ChangeOfPilotDuringBlaBlaBla;
            }
            set
            {
                if (_gN1_DeparturePort_ChangeOfPilotDuringBlaBlaBla == value)
                {
                    return;
                }

                _gN1_DeparturePort_ChangeOfPilotDuringBlaBlaBla = value;
                OnPropertyChanged("GN1_DeparturePort_ChangeOfPilotDuringBlaBlaBla");
                IsDirty = true;
            }
        }

        public string GN1_DeparturePort_GuideToPortEntryVolPage
        {
            get
            {
                return _gN1_DeparturePort_GuideToPortEntryVolPage;
            }
            set
            {
                if (_gN1_DeparturePort_GuideToPortEntryVolPage == value)
                {
                    return;
                }

                _gN1_DeparturePort_GuideToPortEntryVolPage = value;
                OnPropertyChanged("GN1_DeparturePort_GuideToPortEntryVolPage");
                IsDirty = true;
            }
        }

        public string GN1_ArrivalPort_NameOfBerth
        {
            get
            {
                return _gN1_ArrivalPort_NameOfBerth;
            }
            set
            {
                if (_gN1_ArrivalPort_NameOfBerth == value)
                {
                    return;
                }

                _gN1_ArrivalPort_NameOfBerth = value;
                OnPropertyChanged("GN1_ArrivalPort_NameOfBerth");
                IsDirty = true;
            }
        }

        public string GN1_ArrivalPort_VhfChannelPortControl
        {
            get
            {
                return _gN1_ArrivalPort_VhfChannelPortControl;
            }
            set
            {
                if (_gN1_ArrivalPort_VhfChannelPortControl == value)
                {
                    return;
                }

                _gN1_ArrivalPort_VhfChannelPortControl = value;
                OnPropertyChanged("GN1_ArrivalPort_VhfChannelPortControl");
                IsDirty = true;
            }
        }

        public string GN1_ArrivalPort_VhfChannelPilots
        {
            get
            {
                return _gN1_ArrivalPort_VhfChannelPilots;
            }
            set
            {
                if (_gN1_ArrivalPort_VhfChannelPilots == value)
                {
                    return;
                }

                _gN1_ArrivalPort_VhfChannelPilots = value;
                OnPropertyChanged("GN1_ArrivalPort_VhfChannelPilots");
                IsDirty = true;
            }
        }

        public string GN1_ArrivalPort_PreArrivalNoticeFor
        {
            get
            {
                return _gN1_ArrivalPort_PreArrivalNoticeFor;
            }
            set
            {
                if (_gN1_ArrivalPort_PreArrivalNoticeFor == value)
                {
                    return;
                }

                _gN1_ArrivalPort_PreArrivalNoticeFor = value;
                OnPropertyChanged("GN1_ArrivalPort_PreArrivalNoticeFor");
                IsDirty = true;
            }
        }

        public string GN1_ArrivalPort_VhfChannel
        {
            get
            {
                return _gN1_ArrivalPort_VhfChannel;
            }
            set
            {
                if (_gN1_ArrivalPort_VhfChannel == value)
                {
                    return;
                }

                _gN1_ArrivalPort_VhfChannel = value;
                OnPropertyChanged("GN1_ArrivalPort_VhfChannel");
                IsDirty = true;
            }
        }

        public string GN1_ArrivalPort_PreArrivalNoticeForSecondRow
        {
            get
            {
                return _gN1_ArrivalPort_PreArrivalNoticeForSecondRow;
            }
            set
            {
                if (_gN1_ArrivalPort_PreArrivalNoticeForSecondRow == value)
                {
                    return;
                }

                _gN1_ArrivalPort_PreArrivalNoticeForSecondRow = value;
                OnPropertyChanged("GN1_ArrivalPort_PreArrivalNoticeForSecondRow");
                IsDirty = true;
            }
        }

        public string GN1_ArrivalPort_DraughtRestrictions
        {
            get
            {
                return _gN1_ArrivalPort_DraughtRestrictions;
            }
            set
            {
                if (_gN1_ArrivalPort_DraughtRestrictions == value)
                {
                    return;
                }

                _gN1_ArrivalPort_DraughtRestrictions = value;
                OnPropertyChanged("GN1_ArrivalPort_DraughtRestrictions");
                IsDirty = true;
            }
        }

        public string GN1_ArrivalPort_AirDraughtRestrictions
        {
            get
            {
                return _gN1_ArrivalPort_AirDraughtRestrictions;
            }
            set
            {
                if (_gN1_ArrivalPort_AirDraughtRestrictions == value)
                {
                    return;
                }

                _gN1_ArrivalPort_AirDraughtRestrictions = value;
                OnPropertyChanged("GN1_ArrivalPort_AirDraughtRestrictions");
                IsDirty = true;
            }
        }

        public string GN1_ArrivalPort_OtherRestrictions
        {
            get
            {
                return _gN1_ArrivalPort_OtherRestrictions;
            }
            set
            {
                if (_gN1_ArrivalPort_OtherRestrictions == value)
                {
                    return;
                }

                _gN1_ArrivalPort_OtherRestrictions = value;
                OnPropertyChanged("GN1_ArrivalPort_OtherRestrictions");
                IsDirty = true;
            }
        }

        public string GN1_ArrivalPort_ChangeOfPilotDuringBlaBlaBla
        {
            get
            {
                return _gN1_ArrivalPort_ChangeOfPilotDuringBlaBlaBla;
            }
            set
            {
                if (_gN1_ArrivalPort_ChangeOfPilotDuringBlaBlaBla == value)
                {
                    return;
                }

                _gN1_ArrivalPort_ChangeOfPilotDuringBlaBlaBla = value;
                OnPropertyChanged("GN1_ArrivalPort_ChangeOfPilotDuringBlaBlaBla");
                IsDirty = true;
            }
        }

        public string GN1_ArrivalPort_GuideToPortEntryVolPage
        {
            get
            {
                return _gN1_ArrivalPort_GuideToPortEntryVolPage;
            }
            set
            {
                if (_gN1_ArrivalPort_GuideToPortEntryVolPage == value)
                {
                    return;
                }

                _gN1_ArrivalPort_GuideToPortEntryVolPage = value;
                OnPropertyChanged("GN1_ArrivalPort_GuideToPortEntryVolPage");
                IsDirty = true;
            }
        }

        public string GN1_TidalInfoDeparture_Time1LowWater 
        {
            get
            {
                return _gN1_TidalInfoDeparture_Time1LowWater;
            }
            set
            {
                if (_gN1_TidalInfoDeparture_Time1LowWater == value)
                {
                    return;
                }

                _gN1_TidalInfoDeparture_Time1LowWater = value;
                OnPropertyChanged("GN1_TidalInfoDeparture_Time1LowWater");
                IsDirty = true;
            }
        }

        public string GN1_TidalInfoDeparture_Time1HighWater 
        {
            get
            {
                return _gN1_TidalInfoDeparture_Time1HighWater;
            }
            set
            {
                if (_gN1_TidalInfoDeparture_Time1HighWater == value)
                {
                    return;
                }

                _gN1_TidalInfoDeparture_Time1HighWater = value;
                OnPropertyChanged("GN1_TidalInfoDeparture_Time1HighWater");
                IsDirty = true;
            }
        }

        public string GN1_TidalInfoDeparture_Time1Rise 
        {
            get
            {
                return _gN1_TidalInfoDeparture_Time1Rise;
            }
            set
            {
                if (_gN1_TidalInfoDeparture_Time1Rise == value)
                {
                    return;
                }

                _gN1_TidalInfoDeparture_Time1Rise = value;
                OnPropertyChanged("GN1_TidalInfoDeparture_Time1Rise");
                IsDirty = true;
            }
        }

        public string GN1_TidalInfoDeparture_Time2LowWater 
        {
            get
            {
                return _gN1_TidalInfoDeparture_Time2LowWater;
            }
            set
            {
                if (_gN1_TidalInfoDeparture_Time2LowWater == value)
                {
                    return;
                }

                _gN1_TidalInfoDeparture_Time2LowWater = value;
                OnPropertyChanged("GN1_TidalInfoDeparture_Time2LowWater");
                IsDirty = true;
            }
        }

        public string GN1_TidalInfoDeparture_Time2HighWater 
        {
            get
            {
                return _gN1_TidalInfoDeparture_Time2HighWater;
            }
            set
            {
                if (_gN1_TidalInfoDeparture_Time2HighWater == value)
                {
                    return;
                }

                _gN1_TidalInfoDeparture_Time2HighWater = value;
                OnPropertyChanged("GN1_TidalInfoDeparture_Time2HighWater");
                IsDirty = true;
            }
        }

        public string GN1_TidalInfoDeparture_Time2Rise 
        {
            get
            {
                return _gN1_TidalInfoDeparture_Time2Rise;
            }
            set
            {
                if (_gN1_TidalInfoDeparture_Time2Rise == value)
                {
                    return;
                }

                _gN1_TidalInfoDeparture_Time2Rise = value;
                OnPropertyChanged("GN1_TidalInfoDeparture_Time2Rise");
                IsDirty = true;
            }
        }

        public string GN1_TidalInfoDeparture_StandardPort 
        {
            get
            {
                return _gN1_TidalInfoDeparture_StandardPort;
            }
            set
            {
                if (_gN1_TidalInfoDeparture_StandardPort == value)
                {
                    return;
                }

                _gN1_TidalInfoDeparture_StandardPort = value;
                OnPropertyChanged("GN1_TidalInfoDeparture_StandardPort");
                IsDirty = true;
            }
        }

        public string GN1_TidalInfoArrival_Time1LowWater 
        {
            get
            {
                return _gN1_TidalInfoArrival_Time1LowWater;
            }
            set
            {
                if (_gN1_TidalInfoArrival_Time1LowWater == value)
                {
                    return;
                }

                _gN1_TidalInfoArrival_Time1LowWater = value;
                OnPropertyChanged("GN1_TidalInfoArrival_Time1LowWater");
                IsDirty = true;
            }
        }

        public string GN1_TidalInfoArrival_Time1HighWater 
        {
            get
            {
                return _gN1_TidalInfoArrival_Time1HighWater;
            }
            set
            {
                if (_gN1_TidalInfoArrival_Time1HighWater == value)
                {
                    return;
                }

                _gN1_TidalInfoArrival_Time1HighWater = value;
                OnPropertyChanged("GN1_TidalInfoArrival_Time1HighWater");
                IsDirty = true;
            }
        }

        public string GN1_TidalInfoArrival_Time1Rise 
        {
            get
            {
                return _gN1_TidalInfoArrival_Time1Rise;
            }
            set
            {
                if (_gN1_TidalInfoArrival_Time1Rise == value)
                {
                    return;
                }

                _gN1_TidalInfoArrival_Time1Rise = value;
                OnPropertyChanged("GN1_TidalInfoArrival_Time1Rise");
                IsDirty = true;
            }
        }

        public string GN1_TidalInfoArrival_Time2LowWater 
        {
            get
            {
                return _gN1_TidalInfoArrival_Time2LowWater;
            }
            set
            {
                if (_gN1_TidalInfoArrival_Time2LowWater == value)
                {
                    return;
                }

                _gN1_TidalInfoArrival_Time2LowWater = value;
                OnPropertyChanged("GN1_TidalInfoArrival_Time2LowWater");
                IsDirty = true;
            }
        }

        public string GN1_TidalInfoArrival_Time2HighWater 
        {
            get
            {
                return _gN1_TidalInfoArrival_Time2HighWater;
            }
            set
            {
                if (_gN1_TidalInfoArrival_Time2HighWater == value)
                {
                    return;
                }

                _gN1_TidalInfoArrival_Time2HighWater = value;
                OnPropertyChanged("GN1_TidalInfoArrival_Time2HighWater");
                IsDirty = true;
            }
        }

        public string GN1_TidalInfoArrival_Time2Rise 
        {
            get
            {
                return _gN1_TidalInfoArrival_Time2Rise;
            }
            set
            {
                if (_gN1_TidalInfoArrival_Time2Rise == value)
                {
                    return;
                }

                _gN1_TidalInfoArrival_Time2Rise = value;
                OnPropertyChanged("GN1_TidalInfoArrival_Time2Rise");
                IsDirty = true;
            }
        }

        public string GN1_TidalInfoArrival_StandardPort 
        {
            get
            {
                return _gN1_TidalInfoArrival_StandardPort;
            }
            set
            {
                if (_gN1_TidalInfoArrival_StandardPort == value)
                {
                    return;
                }

                _gN1_TidalInfoArrival_StandardPort = value;
                OnPropertyChanged("GN1_TidalInfoArrival_StandardPort");
                IsDirty = true;
            }
        }

        public string GN1_GeneralDescription 
        {
            get
            {
                return _gN1_GeneralDescription;
            }
            set
            {
                if (_gN1_GeneralDescription == value)
                {
                    return;
                }

                _gN1_GeneralDescription = value;
                OnPropertyChanged("GN1_GeneralDescription");
                IsDirty = true;
            }
        }

        public string GN2_NavigationalWarnings 
        {
            get
            {
                return _gN2_NavigationalWarnings;
            }
            set
            {
                if (_gN2_NavigationalWarnings == value)
                {
                    return;
                }

                _gN2_NavigationalWarnings = value;
                OnPropertyChanged("GN2_NavigationalWarnings");
                IsDirty = true;
            }
        }

        public string GN2_EmergencyAnchorages 
        {
            get
            {
                return _gN2_EmergencyAnchorages;
            }
            set
            {
                if (_gN2_EmergencyAnchorages == value)
                {
                    return;
                }

                _gN2_EmergencyAnchorages = value;
                OnPropertyChanged("GN2_EmergencyAnchorages");
                IsDirty = true;
            }
        }

        public string GN2_TAndPNotices 
        {
            get
            {
                return _gN2_TAndPNotices;
            }
            set
            {
                if (_gN2_TAndPNotices == value)
                {
                    return;
                }

                _gN2_TAndPNotices = value;
                OnPropertyChanged("GN2_TAndPNotices");
                IsDirty = true;
            }
        }

        public string GN2_Weather 
        {
            get
            {
                return _gN2_Weather;
            }
            set
            {
                if (_gN2_Weather == value)
                {
                    return;
                }

                _gN2_Weather = value;
                OnPropertyChanged("GN2_Weather");
                IsDirty = true;
            }
        }

        public string GN2_RestrictedAreas 
        {
            get
            {
                return _gN2_RestrictedAreas;
            }
            set
            {
                if (_gN2_RestrictedAreas == value)
                {
                    return;
                }

                _gN2_RestrictedAreas = value;
                OnPropertyChanged("GN2_RestrictedAreas");
                IsDirty = true;
            }
        }

        public string GN2_AdditionalNotes 
        {
            get
            {
                return _gN2_AdditionalNotes;
            }
            set
            {
                if (_gN2_AdditionalNotes == value)
                {
                    return;
                }

                _gN2_AdditionalNotes = value;
                OnPropertyChanged("GN2_AdditionalNotes");
                IsDirty = true;
            }
        }

        public string GN2_Rcc_Station0 
        {
            get
            {
                return _gN2_Rcc_Station0;
            }
            set
            {
                if (_gN2_Rcc_Station0 == value)
                {
                    return;
                }

                _gN2_Rcc_Station0 = value;
                OnPropertyChanged("GN2_Rcc_Station0");
                IsDirty = true;
            }
        }

        public string GN2_Rcc_Remarks0 
        {
            get
            {
                return _gN2_Rcc_Remarks0;
            }
            set
            {
                if (_gN2_Rcc_Remarks0 == value)
                {
                    return;
                }

                _gN2_Rcc_Remarks0 = value;
                OnPropertyChanged("GN2_Rcc_Remarks0");
                IsDirty = true;
            }
        }

        public string GN2_Rcc_ChannelPhone0 
        {
            get
            {
                return _gN2_Rcc_ChannelPhone0;
            }
            set
            {
                if (_gN2_Rcc_ChannelPhone0 == value)
                {
                    return;
                }

                _gN2_Rcc_ChannelPhone0 = value;
                OnPropertyChanged("GN2_Rcc_ChannelPhone0");
                IsDirty = true;
            }
        }

        public string GN2_Rcc_Station1 
        {
            get
            {
                return _gN2_Rcc_Station1;
            }
            set
            {
                if (_gN2_Rcc_Station1 == value)
                {
                    return;
                }

                _gN2_Rcc_Station1 = value;
                OnPropertyChanged("GN2_Rcc_Station1");
                IsDirty = true;
            }
        }

        public string GN2_Rcc_Remarks1 
        {
            get
            {
                return _gN2_Rcc_Remarks1;
            }
            set
            {
                if (_gN2_Rcc_Remarks1 == value)
                {
                    return;
                }

                _gN2_Rcc_Remarks1 = value;
                OnPropertyChanged("GN2_Rcc_Remarks1");
                IsDirty = true;
            }
        }

        public string GN2_Rcc_ChannelPhone1 
        {
            get
            {
                return _gN2_Rcc_ChannelPhone1;
            }
            set
            {
                if (_gN2_Rcc_ChannelPhone1 == value)
                {
                    return;
                }

                _gN2_Rcc_ChannelPhone1 = value;
                OnPropertyChanged("GN2_Rcc_ChannelPhone1");
                IsDirty = true;
            }
        }

        public string GN2_Rcc_Station2 
        {
            get
            {
                return _gN2_Rcc_Station2;
            }
            set
            {
                if (_gN2_Rcc_Station2 == value)
                {
                    return;
                }

                _gN2_Rcc_Station2 = value;
                OnPropertyChanged("GN2_Rcc_Station2");
                IsDirty = true;
            }
        }

        public string GN2_Rcc_Remarks2 
        {
            get
            {
                return _gN2_Rcc_Remarks2;
            }
            set
            {
                if (_gN2_Rcc_Remarks2 == value)
                {
                    return;
                }

                _gN2_Rcc_Remarks2 = value;
                OnPropertyChanged("GN2_Rcc_Remarks2");
                IsDirty = true;
            }
        }

        public string GN2_Rcc_ChannelPhone2 
        {
            get
            {
                return _gN2_Rcc_ChannelPhone2;
            }
            set
            {
                if (_gN2_Rcc_ChannelPhone2 == value)
                {
                    return;
                }

                _gN2_Rcc_ChannelPhone2 = value;
                OnPropertyChanged("GN2_Rcc_ChannelPhone2");
                IsDirty = true;
            }
        }

        public string GN2_Rcc_Station3 
        {
            get
            {
                return _gN2_Rcc_Station3;
            }
            set
            {
                if (_gN2_Rcc_Station3 == value)
                {
                    return;
                }

                _gN2_Rcc_Station3 = value;
                OnPropertyChanged("GN2_Rcc_Station3");
                IsDirty = true;
            }
        }

        public string GN2_Rcc_Remarks3 
        {
            get
            {
                return _gN2_Rcc_Remarks3;
            }
            set
            {
                if (_gN2_Rcc_Remarks3 == value)
                {
                    return;
                }

                _gN2_Rcc_Remarks3 = value;
                OnPropertyChanged("GN2_Rcc_Remarks3");
                IsDirty = true;
            }
        }

        public string GN2_Rcc_ChannelPhone3 
        {
            get
            {
                return _gN2_Rcc_ChannelPhone3;
            }
            set
            {
                if (_gN2_Rcc_ChannelPhone3 == value)
                {
                    return;
                }

                _gN2_Rcc_ChannelPhone3 = value;
                OnPropertyChanged("GN2_Rcc_ChannelPhone3");
                IsDirty = true;
            }
        }

        public string GN2_Rcc_Station4
        {
            get
            {
                return _gN2_Rcc_Station4;
            }
            set
            {
                if (_gN2_Rcc_Station4 == value)
                {
                    return;
                }

                _gN2_Rcc_Station4 = value;
                OnPropertyChanged("GN2_Rcc_Station4");
                IsDirty = true;
            }
        }

        public string GN2_Rcc_Remarks4
        {
            get
            {
                return _gN2_Rcc_Remarks4;
            }
            set
            {
                if (_gN2_Rcc_Remarks4 == value)
                {
                    return;
                }

                _gN2_Rcc_Remarks4 = value;
                OnPropertyChanged("GN2_Rcc_Remarks4");
                IsDirty = true;
            }
        }

        public string GN2_Rcc_ChannelPhone4
        {
            get
            {
                return _gN2_Rcc_ChannelPhone4;
            }
            set
            {
                if (_gN2_Rcc_ChannelPhone4 == value)
                {
                    return;
                }

                _gN2_Rcc_ChannelPhone4 = value;
                OnPropertyChanged("GN2_Rcc_ChannelPhone4");
                IsDirty = true;
            }
        }

        public string GN2_AllChartsCorrectedUpTo_NoticeToMarinersNo 
        {
            get
            {
                return _gN2_AllChartsCorrectedUpTo_NoticeToMarinersNo;
            }
            set
            {
                if (_gN2_AllChartsCorrectedUpTo_NoticeToMarinersNo == value)
                {
                    return;
                }

                _gN2_AllChartsCorrectedUpTo_NoticeToMarinersNo = value;
                OnPropertyChanged("GN2_AllChartsCorrectedUpTo_NoticeToMarinersNo");
                IsDirty = true;
            }
        }

        public string GN2_AllChartsCorrectedUpTo_Dated 
        {
            get
            {
                return _gN2_AllChartsCorrectedUpTo_Dated;
            }
            set
            {
                if (_gN2_AllChartsCorrectedUpTo_Dated == value)
                {
                    return;
                }

                _gN2_AllChartsCorrectedUpTo_Dated = value;
                OnPropertyChanged("GN2_AllChartsCorrectedUpTo_Dated");
                IsDirty = true;
            }
        }

        public string GN2_IsWaterDepthAtChartDatumBlaBlaBla 
        {
            get
            {
                return _gN2_IsWaterDepthAtChartDatumBlaBlaBla;
            }
            set
            {
                if (_gN2_IsWaterDepthAtChartDatumBlaBlaBla == value)
                {
                    return;
                }

                _gN2_IsWaterDepthAtChartDatumBlaBlaBla = value;
                OnPropertyChanged("GN2_IsWaterDepthAtChartDatumBlaBlaBla");
                IsDirty = true;
            }
        }

        public string GN2_IfYesHaveSquatCalculationsBlaBlaBla 
        {
            get
            {
                return _gN2_IfYesHaveSquatCalculationsBlaBlaBla;
            }
            set
            {
                if (_gN2_IfYesHaveSquatCalculationsBlaBlaBla == value)
                {
                    return;
                }

                _gN2_IfYesHaveSquatCalculationsBlaBlaBla = value;
                OnPropertyChanged("GN2_IfYesHaveSquatCalculationsBlaBlaBla");
                IsDirty = true;
            }
        }

        public string GN2_IsOptimizedRouteFromSeawareWeather
        {
            get
            {
                return _gN2_IsOptimizedRouteFromSeawareWeather;
            }
            set
            {
                if (_gN2_IsOptimizedRouteFromSeawareWeather == value)
                {
                    return;
                }

                _gN2_IsOptimizedRouteFromSeawareWeather = value;
                OnPropertyChanged("GN2_IsOptimizedRouteFromSeawareWeather");
                IsDirty = true;
            }
        }

        public string GN2_Officer0 
        {
            get
            {
                return _gN2_Officer0;
            }
            set
            {
                if (_gN2_Officer0 == value)
                {
                    return;
                }

                _gN2_Officer0 = value;
                OnPropertyChanged("GN2_Officer0");
                IsDirty = true;
            }
        }

        public string GN2_Officer1 
        {
            get
            {
                return _gN2_Officer1;
            }
            set
            {
                if (_gN2_Officer1 == value)
                {
                    return;
                }

                _gN2_Officer1 = value;
                OnPropertyChanged("GN2_Officer1");
                IsDirty = true;
            }
        }

        public string GN2_Officer2 
        {
            get
            {
                return _gN2_Officer2;
            }
            set
            {
                if (_gN2_Officer2 == value)
                {
                    return;
                }

                _gN2_Officer2 = value;
                OnPropertyChanged("GN2_Officer2");
                IsDirty = true;
            }
        }

        public string GN2_Officer3  
        {
            get
            {
                return _gN2_Officer3;
            }
            set
            {
                if (_gN2_Officer3 == value)
                {
                    return;
                }

                _gN2_Officer3 = value;
                OnPropertyChanged("GN2_Officer3");
                IsDirty = true;
            }
        }

        public string GN2_Officer4  
        {
            get
            {
                return _gN2_Officer4;
            }
            set
            {
                if (_gN2_Officer4 == value)
                {
                    return;
                }

                _gN2_Officer4 = value;
                OnPropertyChanged("GN2_Officer4");
                IsDirty = true;
            }
        }

        public string GN2_Officer5  
        {
            get
            {
                return _gN2_Officer5;
            }
            set
            {
                if (_gN2_Officer5 == value)
                {
                    return;
                }

                _gN2_Officer5 = value;
                OnPropertyChanged("GN2_Officer5");
                IsDirty = true;
            }
        }

        public string GN2_Master  
        {
            get
            {
                return _gN2_Master;
            }
            set
            {
                if (_gN2_Master == value)
                {
                    return;
                }

                _gN2_Master = value;
                OnPropertyChanged("GN2_Master");
                IsDirty = true;
            }
        }

        public string AN_AdditionalNotes
        {
            get
            {
                return _aN_AdditionalNotes;
            }
            set
            {
                if (_aN_AdditionalNotes == value)
                {
                    return;
                }

                _aN_AdditionalNotes = value;
                OnPropertyChanged("AN_AdditionalNotes");
                IsDirty = true;
            }
        }


        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = System.Threading.Interlocked.CompareExchange(ref PropertyChanged, null, null);
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
