using System;
using System.Windows;
using MahApps.Metro.Controls;
using EcdisLayer;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using System.IO;

//using WPF.Themes;

namespace PassagePlanner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private bool _applicationExitHandled = false;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            this.routeOverviewTab.IsSelected = true;

            About about = new About();
            string versionNumber = about.AssemblyVersion;
            this.Title = "Passage Planner " + versionNumber;

            Closing += (s, e) => ViewModelLocator.Cleanup();
        }

        private void About_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var aboutBox = new About { Owner = this };
            aboutBox.ShowDialog();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            HandleApplicationExit();
        }

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ViewModelLocator locator = new ViewModelLocator();
            
            if (e.RemovedItems != null && e.RemovedItems.Count == 1 && e.RemovedItems[0].GetType() == typeof(System.Windows.Controls.TabItem))
            {
                System.Windows.Controls.TabItem previousTab = (System.Windows.Controls.TabItem)e.RemovedItems[0];
                if (previousTab == vesselTab)
                {
                    if (locator.VesselVM.IsDirty)
                    {
                        MessageBox messageBox = new MessageBox("Do you want to save changes?", "Save Changes?", MessageBoxButton.YesNo, "Save", "Don't save", string.Empty, ButtonType.OK);
                        bool? dialogResult = messageBox.ShowDialog();

                        if (dialogResult == true)
                        {
                            // Save
                            locator.VesselVM.SaveToSettingsFile();
                        }
                        else
                        {
                            // Rollback
                            locator.VesselVM.GetVesselData();
                        }
                    }
                }
            }
        }

        private void CheckRequiredData()
        {
            ViewModelLocator locator = new ViewModelLocator();
            VezzelViewModel vesselVM = locator.VesselVM;
            if (vesselVM.VesselName == string.Empty || vesselVM.VesselBeam < 1.0 || vesselVM.AllBlockCoefficientsAreNull())
            {
                MessageBoxBig messageBox = new MessageBoxBig("Welcome to Seaware Passage Planner!\n\nPlease start with entering some required information in the \nVessel tab, like Vessel name, Vessel beam and block coefficients.\n\nDo you want to enter this information now?", "Welcome", MessageBoxButton.YesNo, "Yes", "No, later", string.Empty, ButtonType.Yes);
                bool? dialogResult = messageBox.ShowDialog();
                if (dialogResult != null && (bool)dialogResult)
                {
                    vesselTab.IsSelected = true;
                }
            }
        }

        private void ApplicationSettings_Click(object sender, RoutedEventArgs e)
        {
            var applicationSettings = new ApplicationSettings { Owner = this };
            applicationSettings.ShowDialog();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            HandleApplicationExit();
        }

        private void HandleApplicationExit()
        {
            if (!_applicationExitHandled)
            {
                ViewModelLocator locator = new ViewModelLocator();
                RouteViewModel routeVM = locator.RouteVM;
                VezzelViewModel vesselVM = locator.VesselVM;

                if (routeVM.IsDirty || vesselVM.IsDirty)
                {
                    MessageBox mb = new MessageBox("Do you want to save changes?", "Save Changes?", MessageBoxButton.YesNo, "Save", "Don't Save", "Cancel", ButtonType.undefined);
                    bool? dialogResult = mb.ShowDialog();
                    mb = null;

                    if (dialogResult != null)
                    {
                        if (dialogResult == true)
                        {
                            if (routeVM.IsDirty)
                            {
                                // Save route data and passage data
                                routeVM.SaveRoute();
                            }
                            if (vesselVM.IsDirty)
                            {
                                // Save Vessel data
                                vesselVM.SaveToSettingsFile();
                            }
                        }
                    }
                }

                // Always save some application settings
                AppSettingsViewModel appSettingsVM = locator.AppSettingsVM;
                appSettingsVM.LatestRouteFilePath = routeVM.FilePath;
                appSettingsVM.LatestTabIndex = mainTabControl.SelectedIndex;
                appSettingsVM.SaveToSettingsFile();

                _applicationExitHandled = true;
                Application.Current.Shutdown();
            }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModelLocator locator = new ViewModelLocator();
            AppSettingsViewModel appSettingsVM = locator.AppSettingsVM;
            RouteViewModel routeVM = locator.RouteVM;
            VezzelViewModel vesselVM = locator.VesselVM;

            appSettingsVM.GetAppSettings();

            if (appSettingsVM.LatestTabIndex != -1)
            {
                mainTabControl.SelectedIndex = appSettingsVM.LatestTabIndex;
            }
            else
            {
                mainTabControl.SelectedIndex = 0;

                // NOTE! Ugly, but alright for now.
                // Set Window in Maximized mode the first time after installation!
                if (this.WindowState != System.Windows.WindowState.Maximized)
                {
                    this.WindowState = System.Windows.WindowState.Maximized;
                }
            }

            if (appSettingsVM.LatestRouteFilePath != string.Empty)
            {
                routeVM.OpenRoute(appSettingsVM.LatestRouteFilePath, true);
            }
        }

        private void MetroWindow_ContentRendered(object sender, EventArgs e)
        {
            //CheckExpirationDate();
            CheckRequiredData();
        }

        //private void CheckExpirationDate()
        //{
        //    DateTime expirationDate = new DateTime(2014, 1, 1);
        //    if (DateTime.Now > expirationDate)
        //    {
        //        MessageBoxBig messageBox = new MessageBoxBig("This test version of Seaware Passage Planner did expire at " + expirationDate.ToString() + "!\n\nPlease uninstall this version of Seaware Passage Planner and install the latest version!", "Expiration Check", MessageBoxButton.OK);
        //        messageBox.ShowDialog();
        //        Application.Current.Shutdown();
        //    }
        //}

        private void ViewSupportedEcdisFormats_Click(object sender, RoutedEventArgs e)
        {
            var viewSupportedEcdisFormats = new ViewSupportedEcdisFormats { Owner = this };
            viewSupportedEcdisFormats.ShowDialog();
        }

        private void UserGuide_Click(object sender, RoutedEventArgs e)
        {
            string userGuideFileName = "Passage Planner User Guide.pdf";

            try
            {
                string usersGuideFullPath = string.Empty;
        
                // Real installation - User Guide is located in the installation directory
                if (File.Exists(userGuideFileName))
                {
                    Process p = new Process();
                    p.StartInfo.FileName = userGuideFileName;
                    p.Start();
                }
                // When ran in Visual Studio
                else if (File.Exists("..\\..\\..\\Setup\\" + userGuideFileName))
                {
                    Process p = new Process();
                    p.StartInfo.FileName = "..\\..\\..\\Setup\\" + userGuideFileName;
                    p.Start();
                }
                else
                {
                    MessageBox mb1 = new MessageBox(userGuideFileName + " was not found", "File not found", MessageBoxButton.OK);
                    mb1.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox mb2 = new MessageBox("User Guide could not be opened. \nPlease check if Adobe Reader or \nsimilar PDF reader is installed.", "Error", MessageBoxButton.OK);
                mb2.ShowDialog();
            }
        }

    
    }
}