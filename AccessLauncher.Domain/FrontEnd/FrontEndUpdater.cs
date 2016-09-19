using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using AccessLauncher.Domain.XmlAccess;

namespace AccessLauncher.Domain.FrontEnd
{
    /* This is the one of the main classes for the front end console application. It consists entirely of static methods.
     * 
     * There are two public methods in this class: UpdateFrontEnd and UpdateLauncher.
     * 
     * UpdateFrontEnd will 
     *      1. recieve a Usertype, get the latest rollout for that type from the backend.xml file, 
     *          and then update the FrontEndXMLFile with that updated info. 
     *      2. It will then delete all local access files in the installed directory. 
     *      3. Finally, it will extract correct rollout file into the correct location.
     * 
     * UpadateLauncher launches the FrontEndInstaller.exe executable within the rollout directory, but it does so using
     * two console arguments:
     *      1. "update" - This instructs the installer to run an update rather than a clean install.
     *      2. the launcher's file path - This instructs the installer which file to run after the update
     */
    public class FrontEndUpdater
    {
        public static void UpdateFrontEnd(DTO.Enums.UserTypeEnum userType)
        {
            updateFrontEndXML(userType);
            deleteAllAccessFiles();
            extractZipFile(userType);
        }

        private static void deleteAllAccessFiles()
        {
            foreach(var file in Directory.GetFiles(GetInfo.GetAppDataPath() + "Access Files"))
            {
                File.Delete(file);
            }
        }

        private static void updateFrontEndXML(DTO.Enums.UserTypeEnum userType)
        {
            DTO.RolloutInfo rollout = GetDataFromXml.GetLatestRollout(userType, DTO.Enums.BackEndOrFrontEndEnum.FrontEnd);
            UpdateXmlFile.UpdateFrontEndXml(rollout);
        }

        private static void extractZipFile(DTO.Enums.UserTypeEnum userType)
        {
            ZipFile.ExtractToDirectory(GetDataFromXml.GetLatestRollout(userType, DTO.Enums.BackEndOrFrontEndEnum.FrontEnd).ZipPath,
                GetInfo.GetAppDataPath() + "\\Access Files\\");
        }

        public static void UpdateLauncher()
        {
            Process installer = new Process();
            installer.StartInfo.FileName = PathManager.GetInstallerFilesDirectory(DTO.Enums.BackEndOrFrontEndEnum.FrontEnd) + "AccessLauncher.FrontEndInstaller.exe";
            installer.StartInfo.Arguments = "update \"" + FrontEnd.GetInfo.GetAppDataPath() + "AccessLauncher.FrontEnd.exe" + "\"";
            installer.StartInfo.UseShellExecute = false;
            installer.StartInfo.WorkingDirectory = PathManager.GetInstallerFilesDirectory(DTO.Enums.BackEndOrFrontEndEnum.FrontEnd);
            installer.Start();
            throw new DTO.Exceptions.QuitConsoleException();
        }

        public static void UpdateOnNextLaunch()
        {
            UpdateXmlFile.SetFrontEndVersionToZero();
        }
    }
}
