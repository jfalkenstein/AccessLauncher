using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using AccessLauncher.Domain.XmlAccess;

namespace AccessLauncher.FrontEndInstaller
{
    /* This is the Front End installer. It is deployed in the same location as the BackEnd. For a user to
     * install the front end, all they need to do is run this simple program, which will implement all they need
     * to run it. This program is a simple console application.
     * 
     * The installer process goes as follows:
     * 1. It obtains the rollout directory from the BackEndSettings.xml file.
     * 2. It creates the necessary directory structure in the user's AppData/Roaming/PBUSA/Access Launcher/ folder.
     * 3. It creates the frontend.xml file in the newly created directory structure.
     * 4. It copies all the neccessary programs, files, and dlls into the directory structure from the Back End folder.
     * 5. It creates a desktop shortcut to the Access Launcher.
     */
    class Program
    {
        
        static void Main(string[] args)
        {
            InstallActionEnum installAction = InstallActionEnum.FreshInstall;
            string launcherLocation = "";
            if (args.Length != 0)
            {
                if (args[0] == "update")
                {
                    installAction = InstallActionEnum.Update;
                    launcherLocation = args[1];
                }
            }
            string rolloutDirectory = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.FullName + "\\";
            createDirectories();
            copyFilesToAppDataFolder();
            int currentLauncherVersion = Domain.FrontEndInstaller.InstallerService.GetCurrentLauncherVersion();
            if (installAction == InstallActionEnum.Update)
            {
                Domain.FrontEndInstaller.InstallerService.UpdateFrontEndLauncherVersion(currentLauncherVersion);
                Domain.FrontEndInstaller.InstallerService.LaunchFrontEnd(launcherLocation); ;
            }
            else
            {
                CreateXmlFile.CreateFrontEndXMLFile(rolloutDirectory, Domain.FrontEnd.GetInfo.GetAppDataPath(),currentLauncherVersion);
                createDesktopShortcut();
            }
        }

        private static void createDesktopShortcut()
        {
            string appDataPath = Domain.FrontEnd.GetInfo.GetAppDataPath();
            IWshRuntimeLibrary.WshShell wsh = new IWshRuntimeLibrary.WshShell();
            IWshRuntimeLibrary.IWshShortcut shortcut = wsh.CreateShortcut(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\PB Database.lnk");
            shortcut.TargetPath = appDataPath + "AccessLauncher.FrontEnd.exe";
            shortcut.WorkingDirectory = appDataPath;
            shortcut.IconLocation = appDataPath + "p_b_blue.ico";
            shortcut.Save();
        }

        private static void copyFilesToAppDataFolder()
        {
            List<DTO.InstallerItem> installFiles = Domain.FrontEndInstaller.InstallerService.GetInstallerFiles();
            foreach(var file in installFiles)
            {
                file.ExecuteCopy();
            }
        }

        static void createDirectories()
        {
            Directory.CreateDirectory(Domain.FrontEnd.GetInfo.GetAppDataPath() + "Access Files");
        }
    }
}
