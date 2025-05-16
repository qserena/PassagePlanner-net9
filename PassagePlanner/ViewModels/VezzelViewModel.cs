using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Collections.Specialized;
using System.Collections;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.ComponentModel;
using System.Threading;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using EcdisLayer;
using System.Configuration;

namespace PassagePlanner
{
    /// <summary>
    /// 2014-07-27: Had to make a copy of this class (previously called VesselViewModel) and name it VezzelViewModel.
    /// The reason is that I previously used VesselViewModel not only as the view model, but also serialized the class
    /// to write and read from file VesselData.xml. This was probably a bad idea - too complicated and probably was the
    /// reason to a bug where the two DispatchingObservableCollections sometimes was not written to VesselData.xml.
    /// To preserve back compatibility I had to keep a class called VesselViewModel (because the root element of VesselData.xml
    /// in Passage Planner version 1.0.2 is "VesselViewModel"). However, I changed VesselViewModel from being a view model
    /// into being a simple container class to be serialized. Current implementation reads VesselData.xml files with both
    /// root element VesselViewModel (old files) and VesselData (new files). Current implementation only writes VesselData.xml
    /// containing root element VesselData.
    /// Suggestion for the future: Remove VesselViewModel (the one in directory "Model") when no VesselData.xml out on the vessels 
    /// should contain root element VesselViewModel and rename this class VezzelViewModel back to its original name VesselViewModel.
    /// rename VezzelViewModel 
    /// </summary>
    public class VezzelViewModel : ViewModelBase
    {
        private bool _isDirty;
        private string _vesselName;
        private string _vesselOwner;
        private double _vesselBeam;
        private double _minUkcRequired;
        private DispatchingObservableCollection<NavigationalBridgeWatchCondition> _navigationalBridgeWatchConditions;
        private DispatchingObservableCollection<BlockCoefficientAtDraught> _blockCoefficients;
        private List<string> _availableEcdisSystems = new List<string>();
        private string _selectedEcdisSystem = string.Empty;
        private string _statusText;
        private static readonly XmlSerializer _serializerReadVersion102 = new XmlSerializer(typeof(VesselViewModel));
        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(VesselData));

        private string _vesselDataFilePath = Path.Combine(FileManager.SettingsDirectory, "VesselData.xml");

        public RelayCommand SaveVesselDataCommand { get; private set; }

        /// <summary>
        /// Initializes a new instance of the VesselViewModel class.
        /// </summary>
        public VezzelViewModel()
        {
            SaveVesselDataCommand = new RelayCommand(SaveToSettingsFile);

            GetVesselData();
        }


        public void GetVesselData()
        {
            if (File.Exists(_vesselDataFilePath))
            {
                // Read saved values from settings file
                ReadFromSettingsFile();
            }
           
            if (this.NavigationalBridgeWatchConditions == null || this.NavigationalBridgeWatchConditions.Count == 0)
            {
                InitializeNavigationalBridgeWatchConditions();
                // Set default values if values in file are empty strings.
                // This is essentially for the first time of use.
                FillNavigationalBridgeWatchConditionsWithDefaultValues();
            }

            if (this.BlockCoefficients == null || this.BlockCoefficients.Count == 0)
            {
                InitializeBlockCoefficients();
            }
        }

        public bool IsDirty
        { 
            get
            {
                return _isDirty || BlockCoefficientAtDraught.IsDirty || NavigationalBridgeWatchCondition.IsDirty;
            }
            set
            {
                _isDirty = value;
                if (value == false)
                {
                    // Reset IsDirty flag in BlockCoefficientAtDraught and NavigationalBridgeWatchCondition
                    BlockCoefficientAtDraught.IsDirty = false;
                    NavigationalBridgeWatchCondition.IsDirty = false;
                }
                RaisePropertyChanged("IsDirty");
            }
        }

        public string VesselName 
        {
            get
            {
                return _vesselName;
            }
            set
            {
                if (_vesselName == value)
                {
                    return;
                }

                _vesselName = value;
                IsDirty = true;
                RaisePropertyChanged("VesselName");
            }
        }

        public string VesselOwner
        {
            get
            {
                return _vesselOwner;
            }
            set
            {
                if (_vesselOwner == value)
                {
                    return;
                }

                _vesselOwner = value;
                IsDirty = true;
                RaisePropertyChanged("VesselOwner");
            }
        }

        public double VesselBeam
        {
            get
            {
                return _vesselBeam;
            }
            set
            {
                if (_vesselBeam == value)
                {
                    return;
                }

                _vesselBeam = value;
                IsDirty = true;
                RaisePropertyChanged("VesselBeam");
            }
        }

        public double MinUkcRequired
        {
            get
            {
                return _minUkcRequired;
            }
            set
            {
                if (_minUkcRequired == value)
                {
                    return;
                }

                _minUkcRequired = value;
                IsDirty = true;
                RaisePropertyChanged("MinUkcRequired");
            }
        }

        public DispatchingObservableCollection<NavigationalBridgeWatchCondition> NavigationalBridgeWatchConditions
        {
            get
            {
                return _navigationalBridgeWatchConditions;
            }
            set
            {
                if (_navigationalBridgeWatchConditions == value)
                {
                    return;
                }

                _navigationalBridgeWatchConditions = value;
                IsDirty = true;
                RaisePropertyChanged("NavigationalBridgeWatchConditions");
            }
        }


        public DispatchingObservableCollection<BlockCoefficientAtDraught> BlockCoefficients
        {
            get
            {
                return _blockCoefficients;
            }
            set
            {
                if (_blockCoefficients == value)
                {
                    return;
                }

                _blockCoefficients = value;
                IsDirty = true;
                RaisePropertyChanged("BlockCoefficients");
            }
        }

        public string StatusBarText
        {
            get
            {
                return _statusText;
            }
            set
            {
                _statusText = value;
                RaisePropertyChanged("StatusBarText");
            }
        }
        

        private void ReadFromSettingsFile() 
        {
            FileStream fileStream = null;
            string rootName = string.Empty;

            try
            {
                // First read the ROOT element of the xml file, to decide if it is an old or a new xml format
                XmlDocument doc = new XmlDocument();
                doc.Load(_vesselDataFilePath);
                XmlElement root = doc.DocumentElement;
                rootName = root.Name;
       
                fileStream = new FileStream(_vesselDataFilePath, FileMode.Open, FileAccess.Read, FileShare.None);

                switch (rootName)
                {
                    case "VesselViewModel":

                        // OLD xml format with root element VesselViewModel (up to Passage Planner version 1.0.2)
                        XmlSerializer vesselViewModelSerializer = new XmlSerializer(typeof(VesselViewModel));

                        // Create a temporary VesselViewModel
                        VesselViewModel vesselViewModel = new VesselViewModel();
                        vesselViewModel = (VesselViewModel)vesselViewModelSerializer.Deserialize(fileStream);

                        VesselName = vesselViewModel.VesselName;
                        VesselBeam = vesselViewModel.VesselBeam;
                        MinUkcRequired = vesselViewModel.MinUkcRequired;
                        BlockCoefficients = vesselViewModel.BlockCoefficients;
                        NavigationalBridgeWatchConditions = vesselViewModel.NavigationalBridgeWatchConditions;

                        IsDirty = false;
                        break;

                    case "VesselData":

                        // NEW xml format with root element VesselData (from Passage Planner version 1.0.3)
                        XmlSerializer vesselDataSerializer = new XmlSerializer(typeof(VesselData));

                        VesselData vesselData = new VesselData();
                        vesselData = (VesselData)vesselDataSerializer.Deserialize(fileStream);

                        this.VesselName = vesselData.VesselName;
                        this.VesselBeam = vesselData.VesselBeam;
                        this.MinUkcRequired = vesselData.MinUkcRequired;
                        this.BlockCoefficients = new DispatchingObservableCollection<BlockCoefficientAtDraught>(vesselData.BlockCoefficients);
                        this.NavigationalBridgeWatchConditions = new DispatchingObservableCollection<NavigationalBridgeWatchCondition>(vesselData.NavigationalBridgeWatchConditions);

                        IsDirty = false;

                        break;

                    default:
                        MessageBox mb = new MessageBox("Vessel Data could not be read from file " + _vesselDataFilePath + "\nRoot element " + rootName + " is not supported", "ERROR", MessageBoxButton.OK);
                        mb.ShowDialog();
                        break;
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.Show(ex);
            }
            finally
            {
                fileStream.Close();
            }

        }

        public void SaveToSettingsFile()
        {
            try
            {
                // Copy to VesselData object before serialization
                VesselData vesselData = new VesselData();
                vesselData.VesselName = this.VesselName;
                vesselData.VesselBeam = this.VesselBeam;
                vesselData.MinUkcRequired = this.MinUkcRequired;
                vesselData.NavigationalBridgeWatchConditions = new List<NavigationalBridgeWatchCondition>(this.NavigationalBridgeWatchConditions);
                vesselData.BlockCoefficients = new List<BlockCoefficientAtDraught>(this.BlockCoefficients);

                using (FileStream fs = new FileStream(_vesselDataFilePath, FileMode.Create))
                {
                    XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                    xmlWriterSettings.NewLineOnAttributes = true;
                    xmlWriterSettings.Indent = true;

                    using (XmlWriter writer = XmlTextWriter.Create(fs, xmlWriterSettings))
                    {
                        _serializer.Serialize(writer, vesselData);
                    }
                }

                try
                {
                    // Grant access so everybody can read and write to this file
                    FileManager.GrantAccess(_vesselDataFilePath);
                }
                catch (Exception)
                {
                    // Ignore
                }
                

                IsDirty = false;
                StatusBarText = "Vessel data was successfully saved";
            }
            catch (Exception ex)
            {
                ErrorHandler.Show(ex);
            }
        }

        public bool AllBlockCoefficientsAreNull()
        {
            if (_blockCoefficients == null || _blockCoefficients.Count == 0)
            {
                return true;
            }
            else
            {
                foreach (BlockCoefficientAtDraught blockCoefficientAtDraught in _blockCoefficients)
                {
                    if (blockCoefficientAtDraught != null && blockCoefficientAtDraught.BlockCoefficient != null)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Returns the "best" block coefficient Cb for the given draught parameter,
        /// that is, the block coefficient which corresponds to the draught of the block coefficient
        /// that is nearest the given draught parameter.
        /// </summary>
        /// <param name="draught"></param>
        /// <returns></returns>
        public double GetBlockCoefficientAtDraught(double draught)
        {
            double returnValue = 0.0;

            int index = 0;

            if (_blockCoefficients == null || _blockCoefficients.Count == 0)
            {
                return 0.0;
            }

            // Collect the non-null values in a list
            List<BlockCoefficientAtDraught> blockCoefficients = new List<BlockCoefficientAtDraught>();
            foreach (BlockCoefficientAtDraught coefficient in _blockCoefficients)
            {
                if (coefficient.BlockCoefficient != null)
                {
                    blockCoefficients.Add(coefficient);
                }
            }

            if (blockCoefficients.Count == 0)
            {
                return 0.0;
            }

            // Traverse "down" the list of block coefficients as long as we haven't reached the given draught.
            while (index < blockCoefficients.Count - 1 && draught > blockCoefficients[index].MeanDraught)
            {
                index++;
            }

            // If draught > element with index 0
            if (index == 0)
            {
                return (double)blockCoefficients[index].BlockCoefficient;
            }
            
            // Check which coefficient is the best/nearest - index or index - 1
            if ( Math.Abs(draught - blockCoefficients[index - 1].MeanDraught) < Math.Abs(draught - blockCoefficients[index].MeanDraught))
            {
                returnValue = (double)blockCoefficients[index - 1].BlockCoefficient;
            }
            else
            {
                returnValue = (double)blockCoefficients[index].BlockCoefficient;
            }

            return returnValue;
        }

        private void InitializeNavigationalBridgeWatchConditions()
        {
            // Initialize NavigationalBridgeWatchConditions with empty values
            List<NavigationalBridgeWatchCondition> navigationalBridgeWatchConditionList =
                new List<NavigationalBridgeWatchCondition>(new NavigationalBridgeWatchCondition[8] { new NavigationalBridgeWatchCondition(string.Empty, string.Empty), 
                                                                                                     new NavigationalBridgeWatchCondition(string.Empty, string.Empty), 
                                                                                                     new NavigationalBridgeWatchCondition(string.Empty, string.Empty), 
                                                                                                     new NavigationalBridgeWatchCondition(string.Empty, string.Empty), 
                                                                                                     new NavigationalBridgeWatchCondition(string.Empty, string.Empty), 
                                                                                                     new NavigationalBridgeWatchCondition(string.Empty, string.Empty), 
                                                                                                     new NavigationalBridgeWatchCondition(string.Empty, string.Empty), 
                                                                                                     new NavigationalBridgeWatchCondition(string.Empty, string.Empty)});

            _navigationalBridgeWatchConditions =
                new DispatchingObservableCollection<NavigationalBridgeWatchCondition>(navigationalBridgeWatchConditionList);
        }

        private void InitializeBlockCoefficients()
        {
            // Initialize BlockCoefficients with empty values
            List<BlockCoefficientAtDraught> blockCoefficientList =
                new List<BlockCoefficientAtDraught>(new BlockCoefficientAtDraught[31] { new BlockCoefficientAtDraught(0, null), 
                                                                                        new BlockCoefficientAtDraught(1, null), 
                                                                                        new BlockCoefficientAtDraught(2, null),  
                                                                                        new BlockCoefficientAtDraught(3, null), 
                                                                                        new BlockCoefficientAtDraught(4, null), 
                                                                                        new BlockCoefficientAtDraught(5, null), 
                                                                                        new BlockCoefficientAtDraught(6, null), 
                                                                                        new BlockCoefficientAtDraught(7, null), 
                                                                                        new BlockCoefficientAtDraught(8, null),  
                                                                                        new BlockCoefficientAtDraught(9, null),  
                                                                                        new BlockCoefficientAtDraught(10, null),  
                                                                                        new BlockCoefficientAtDraught(11, null), 
                                                                                        new BlockCoefficientAtDraught(12, null),  
                                                                                        new BlockCoefficientAtDraught(13, null), 
                                                                                        new BlockCoefficientAtDraught(14, null),  
                                                                                        new BlockCoefficientAtDraught(15, null),  
                                                                                        new BlockCoefficientAtDraught(16, null),  
                                                                                        new BlockCoefficientAtDraught(17, null),  
                                                                                        new BlockCoefficientAtDraught(18, null), 
                                                                                        new BlockCoefficientAtDraught(19, null),  
                                                                                        new BlockCoefficientAtDraught(20, null), 
                                                                                        new BlockCoefficientAtDraught(21, null), 
                                                                                        new BlockCoefficientAtDraught(22, null),  
                                                                                        new BlockCoefficientAtDraught(23, null), 
                                                                                        new BlockCoefficientAtDraught(24, null),  
                                                                                        new BlockCoefficientAtDraught(25, null),  
                                                                                        new BlockCoefficientAtDraught(26, null),  
                                                                                        new BlockCoefficientAtDraught(27, null), 
                                                                                        new BlockCoefficientAtDraught(28, null), 
                                                                                        new BlockCoefficientAtDraught(29, null), 
                                                                                        new BlockCoefficientAtDraught(30, null)});

            _blockCoefficients = new DispatchingObservableCollection<BlockCoefficientAtDraught>(blockCoefficientList);
        }

        // Default values, which will only be set the first time, until user sets any other value(s).
        private void FillNavigationalBridgeWatchConditionsWithDefaultValues()
        {
            if (NavigationalBridgeWatchConditions[0].Abbreviation == string.Empty &&
                NavigationalBridgeWatchConditions[1].Abbreviation == string.Empty &&
                NavigationalBridgeWatchConditions[2].Abbreviation == string.Empty &&
                NavigationalBridgeWatchConditions[3].Abbreviation == string.Empty &&
                NavigationalBridgeWatchConditions[4].Abbreviation == string.Empty &&
                NavigationalBridgeWatchConditions[5].Abbreviation == string.Empty &&
                NavigationalBridgeWatchConditions[6].Abbreviation == string.Empty &&
                NavigationalBridgeWatchConditions[7].Abbreviation == string.Empty &&
                NavigationalBridgeWatchConditions[0].DefinitionText == string.Empty &&
                NavigationalBridgeWatchConditions[1].DefinitionText == string.Empty &&
                NavigationalBridgeWatchConditions[2].DefinitionText == string.Empty &&
                NavigationalBridgeWatchConditions[3].DefinitionText == string.Empty &&
                NavigationalBridgeWatchConditions[4].DefinitionText == string.Empty &&
                NavigationalBridgeWatchConditions[5].DefinitionText == string.Empty &&
                NavigationalBridgeWatchConditions[6].DefinitionText == string.Empty &&
                NavigationalBridgeWatchConditions[7].DefinitionText == string.Empty)
            {
                NavigationalBridgeWatchConditions[0].Abbreviation = "A";
                NavigationalBridgeWatchConditions[1].Abbreviation = "B";
                NavigationalBridgeWatchConditions[2].Abbreviation = "C";
                NavigationalBridgeWatchConditions[3].Abbreviation = "D";
                NavigationalBridgeWatchConditions[4].Abbreviation = "E";
                NavigationalBridgeWatchConditions[5].Abbreviation = "F";
                NavigationalBridgeWatchConditions[6].Abbreviation = "G";
                NavigationalBridgeWatchConditions[7].Abbreviation = "H";
                NavigationalBridgeWatchConditions[0].DefinitionText = "OOW";
                NavigationalBridgeWatchConditions[1].DefinitionText = "OOW + Lookout";
                NavigationalBridgeWatchConditions[2].DefinitionText = "OOW, Master + Lookout";
                NavigationalBridgeWatchConditions[3].DefinitionText = "OOW, Master, Lookout + Helmsman";
                NavigationalBridgeWatchConditions[4].DefinitionText = "OOW, Master, Pilot";
                NavigationalBridgeWatchConditions[5].DefinitionText = "OOW, Master, Pilot, Helmsman and Lookout";
                NavigationalBridgeWatchConditions[6].DefinitionText = "OOW, Master, 2 Lookouts";
                NavigationalBridgeWatchConditions[7].DefinitionText = "2 OOW, Master, 2 Lookouts";
            }
        }
    }
}