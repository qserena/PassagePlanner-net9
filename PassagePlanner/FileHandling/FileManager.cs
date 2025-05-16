using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.AccessControl;
using System.Security.Principal;

namespace PassagePlanner
{
    public static class FileManager
    {
        private static string dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Seaware");
        private static string applicationDirectory = Path.Combine(dataDirectory, "Passage Planner");
   
        public static string RouteExtension
        {
            get
            {
                return ".ppf";
            }
        }

        public static string ApplicationDirectory
        {
            get
            {
                return applicationDirectory;
            }
        }


        public static string SettingsDirectory
        {
            get
            {
                return Path.Combine(applicationDirectory, "Settings");
            }
        }

        public static string RoutesDirectory 
        {
            get
            {
                string directory = string.Empty;
                ViewModelLocator locator = new ViewModelLocator();
                return locator.AppSettingsVM.RouteFilesDirectory;
            }
        }


        public static bool GrantAccess(string fullPath)
        {
            DirectoryInfo dInfo = new DirectoryInfo(fullPath);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);
            return true;
        }

        public static string FindDirectory(string dir)
        {
            string executableDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Look in executable directory (typical real installation)
            if (Directory.Exists(executableDirectory + dir))
            {
                return string.Format(@"{0}" + dir + "\\", executableDirectory);
            }
            // Look in directory two levels up (when ran in visual studio, typical debug mode) 
            else if (Directory.Exists(executableDirectory + "..\\..\\" + dir))
            {
                return string.Format(@"{0}..\..\" + dir + "\\", executableDirectory);
            }
            // Look in directory two levels up and then in Views directory 
            // (when ran in visual studio, typical debug mode when looking for ReportTemplates in Views directory) 
            else if (Directory.Exists(executableDirectory + "..\\..\\Views\\" + dir))
            {
                return string.Format(@"{0}..\..\Views\" + dir + "\\", executableDirectory);
            }
            else
            {
                throw new Exception("Directory " + dir + " not found");
            }
        }
    }
}
