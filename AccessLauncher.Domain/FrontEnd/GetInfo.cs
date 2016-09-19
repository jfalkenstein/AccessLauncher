using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using AccessLauncher.Domain.XmlAccess;

namespace AccessLauncher.Domain.FrontEnd
{
    /* This class consists entirely of static methods used to obtain useful information for the Front End
     * (and occasionally the Front End Installer). See comments on individual methods for explanations. 
     */
    
    public class GetInfo
    {
        //This gets the location of the user's local AccessLauncher directory, where all files and folders for the
        //launcher are kept.
        public static string GetAppDataPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PBUSA\\Access Launcher\\";
        }
        //This obtains the filepath of the specific file the launcher will open.
        public static string GetLaunchFilePath()
        {
            return GetAppDataPath() + "\\Access Files\\" + GetDataFromXml.GetLaunchFileName();
        }

        //This obtains the path for the Launcher's uninstaller within the rollout directory.
        public static string GetUninstallerPath()
        {
            return PathManager.GetUnininstallerPath();
        }

        //This obtains the current version number for the launcher installed on the user's local computer.
        public static int GetFrontEndLauncherVersion()
        {
            return GetDataFromXml.GetFrontEndLauncherVersion();
        }

        //This will obtain the current launcher version as it is presently pushed out by the Back End manager.
        public static int GetCurrentLauncherVersion()
        {
            return GetDataFromXml.GetCurrentLauncherVersion(PathManager.GetInstallerFilesDirectory(DTO.Enums.BackEndOrFrontEndEnum.FrontEnd));
        }

        //This will return true or false depending on whether a global lockout is currently enabled.
        public static bool LockoutIsEnabled()
        {
            return GetDataFromXml.LockoutIsEnabled(DTO.Enums.BackEndOrFrontEndEnum.FrontEnd);
        }
    }
}
