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
using System.Configuration;
using System.Windows.Controls;

namespace PassagePlanner
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    [Serializable]
    public class AppSettingsViewModel : ViewModelBase
    {
        private string _routeFilesDirectory = string.Empty;
        private string _totalTideDirectory = string.Empty;
        private string _statusText = string.Empty;

        // "Hidden" properties which are written automatically (cannot be set by user)
        private string _latestRouteFilePath = string.Empty;
        private bool _showWaypointsInMap = true;
        private int _latestTabIndex = 0;

        


        [XmlIgnore]
        public RelayCommand SaveAppSettingsCommand { get; private set; }
        [XmlIgnore]
        public RelayCommand BrowseRouteDirectoryCommand { get; private set; }
        [XmlIgnore]
        public RelayCommand BrowseTotalTideDirectoryCommand { get; private set; }

        [XmlIgnore]
        private string _appSettingsFilePath = Path.Combine(FileManager.SettingsDirectory, "AppSettings.xml");


        /// <summary>
        /// Initializes a new instance of the AppSettingsViewModel class.
        /// </summary>
        public AppSettingsViewModel()
        {
            BrowseRouteDirectoryCommand = new RelayCommand(BrowseRouteDirectory);
            BrowseTotalTideDirectoryCommand = new RelayCommand(BrowseTotalTideDirectory);
            SaveAppSettingsCommand = new RelayCommand(SaveToSettingsFile);
        }

        public void GetAppSettings()
        {
            if (File.Exists(_appSettingsFilePath))
            {
                // Read saved values from settings file
                ReadFromSettingsFile();
            }
            else
            {
                
                SetDefaultValues();
            }
        }

        public string RouteFilesDirectory
        {
            get
            {
                return _routeFilesDirectory;
            }
            set
            {
                if (value == _routeFilesDirectory)
                {
                    return;
                }

                _routeFilesDirectory = value;
                RaisePropertyChanged("RouteFilesDirectory");
            }
        }


        public string TotalTideDirectory
        {
            get
            {  
                return _totalTideDirectory;
            }
            set
            {
                if (value == _totalTideDirectory)
                {
                    return;
                }

                _totalTideDirectory = value;
                RaisePropertyChanged("TotalTideDirectory");
            }
        }

        // "Hidden" properties which are written automatically (cannot be set by user)
        public string LatestRouteFilePath
        {
            get
            {
                return _latestRouteFilePath;
            }
            set
            {
                if (value == _latestRouteFilePath)
                {
                    return;
                }

                _latestRouteFilePath = value;
                RaisePropertyChanged("LatestRouteFilePath");
            }
        }


        public bool ShowWaypointsInMap
        {
            get
            {
                return _showWaypointsInMap;
            }
            set
            {
                if (value == _showWaypointsInMap)
                {
                    return;
                }

                _showWaypointsInMap = value;
                RaisePropertyChanged("ShowWaypointsInMap");
            }
        }

        public int LatestTabIndex 
        {
            get
            {
                return _latestTabIndex;
            }
            set
            {
                if (value == _latestTabIndex)
                {
                    return;
                }

                _latestTabIndex = value;
                RaisePropertyChanged("LatestTabIndex");
            }
        }

        [XmlIgnore]
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
        
        public void ReadFromSettingsFile() 
        {
            FileStream fileStream = null;

            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(AppSettingsViewModel));
                fileStream = new FileStream(_appSettingsFilePath, FileMode.Open);

                // Create a temporary AppSettingsViewModel instance, because 
                // you cannot deserialize and assign "this" instance.
                AppSettingsViewModel tempContainer = new AppSettingsViewModel();
                tempContainer = (AppSettingsViewModel)xmlSerializer.Deserialize(fileStream);

                RouteFilesDirectory = tempContainer.RouteFilesDirectory;
                TotalTideDirectory = tempContainer.TotalTideDirectory;

                LatestRouteFilePath = tempContainer.LatestRouteFilePath;
                ShowWaypointsInMap = tempContainer.ShowWaypointsInMap;
                LatestTabIndex = tempContainer.LatestTabIndex;
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

        
        /// <summary>
        /// Serialize application settings and write to file AppSettings.xml.
        /// </summary>
        public void SaveToSettingsFile()
        {
            Stream fs = null;
            XmlWriter writer = null;

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(AppSettingsViewModel));

                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                xmlWriterSettings.NewLineOnAttributes = true;
                xmlWriterSettings.Indent = true;

                // Create an XmlTextWriter using a FileStream.
                fs = new FileStream(_appSettingsFilePath, FileMode.Create);

                writer = XmlTextWriter.Create(fs, xmlWriterSettings);

                // Serialize using the XmlTextWriter.
                serializer.Serialize(writer, this);

                StatusBarText = "Application Settings were successfully saved";
            }
            catch (Exception ex)
            {
                ErrorHandler.Show(ex);
            }
            finally
            {
                writer.Close();
                fs.Close();
            }

            // Grant access so everybody can read and write to this file
            FileManager.GrantAccess(_appSettingsFilePath);    
        }

        private void BrowseRouteDirectory()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.RootFolder = Environment.SpecialFolder.MyComputer;
            dialog.SelectedPath = RouteFilesDirectory;
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            RouteFilesDirectory = dialog.SelectedPath;
        }

        private void BrowseTotalTideDirectory()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.RootFolder = Environment.SpecialFolder.MyComputer;
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            TotalTideDirectory = dialog.SelectedPath;
        }

        private void SetDefaultValues()
        {
            LatestTabIndex = -1;

            if (_totalTideDirectory == string.Empty)
            {
                // Windows 7
                string programFilesX86Dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Admiralty Digital Publications\Admiralty TotalTide");
                if (Directory.Exists(programFilesX86Dir))
                {
                    TotalTideDirectory = programFilesX86Dir;
                }
                else
                {
                    // Windows XP
                    string programFilesDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Admiralty Digital Publications\Admiralty TotalTide");
                    if (Directory.Exists(programFilesDir))
                    {
                        TotalTideDirectory = programFilesDir;
                    }
                }
            }

            if (_routeFilesDirectory == string.Empty)
            {
                _routeFilesDirectory = Path.Combine(FileManager.ApplicationDirectory, "Routes");
                RaisePropertyChanged("RouteFilesDirectory");
            }

            ShowWaypointsInMap = true;

        }
   
    }
}