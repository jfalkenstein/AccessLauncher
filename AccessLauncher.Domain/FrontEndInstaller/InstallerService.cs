using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using AccessLauncher.Domain.XmlAccess;

namespace AccessLauncher.Domain.FrontEndInstaller
{
    /* This class is the primary class used by the Front End Installer. It consists entirely of
     * static methods.
     * 
     * There are 4 public methods in this class: GetInstallerFiles, UpdateFrontEndLauncherVersion, and GetCurrentLauncherVersion,
     *  and LaunchFrontEnd.
     *  
     * GetInstallerFiles will return a List<DTO.InstallerItem> from the current installer manifest.
     * 
     * UpdateFrontEndLauncherVersion will set the current launcher version on the frontend.xml file.
     * 
     * GetCurrentLauncherVersion will return the version number of the current launcher version from the current
     *  installer manifest.
     *  
     * LaunchFrontEnd is used by the FrontEndInstaller only when launcher update is being done. Once the update is complete,
     *  this method is called. It will simply launch the FrontEnd launcher, located by the passed in launcherLocation string.
     * 
     */
    public class InstallerService
    {
        public static List<DTO.InstallerItem> GetInstallerFiles()
        {
            string currentDirectory = Directory.GetCurrentDirectory() + "\\";
            string destinationDirectory = Domain.FrontEnd.GetInfo.GetAppDataPath();
            return GetDataFromXml.GetInstallerManifest(currentDirectory, destinationDirectory);
        }

        public static void UpdateFrontEndLauncherVersion(int currentLauncherVersion)
        {
            UpdateXmlFile.UpdateFrontEndLauncherVersion(currentLauncherVersion);
        }

        public static int GetCurrentLauncherVersion()
        {
            string installerDirectory = Directory.GetCurrentDirectory() + "\\";
            return GetDataFromXml.GetCurrentLauncherVersion(installerDirectory);
        }

        public static void LaunchFrontEnd(string launcherLocation)
        {
            Process launcher = new Process();
            launcher.StartInfo.FileName = launcherLocation;
            launcher.StartInfo.UseShellExecute = false;
            launcher.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(launcherLocation);
            launcher.Start();
        }
    }
}
