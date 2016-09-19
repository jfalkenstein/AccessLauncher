using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccessLauncher.XML;
using System.IO;
using System.Management;

namespace AccessLauncher.Domain
{
    /* This class provides centralized access to the common filePaths needed throughout the various layers of this application.
     * Most of these methods are simple static calls to their partner methods within the XML layer. For explanations on these
     * methods, see their called method within XML.GetPath.
     */
    public class PathManager
    {
        //This function provides the template path for the specified userType.
        public static string GetTemplatePath(DTO.Enums.UserTypeEnum userType)
        {
            return GetPath.GetRolloutDirectoryPath(DTO.Enums.BackEndOrFrontEndEnum.BackEnd)
                + @"Front End Templates\"
                + Enum.GetName(typeof(DTO.Enums.UserTypeEnum), userType);
        }

        public static string GetRolloutDirectoryPath(DTO.Enums.BackEndOrFrontEndEnum whichEnd)
        {
            try
            {
                return GetPath.GetRolloutDirectoryPath(whichEnd);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string GetSettingsXMLPath(DTO.Enums.BackEndOrFrontEndEnum whichSettings)
        {
            return XML.GetPath.GetSettingsXMLPath(whichSettings);
        }

        public static string GetBackEndXmlPath(DTO.Enums.BackEndOrFrontEndEnum whichEnd)
        {
            return XML.GetPath.GetBackEndXMLPath(whichEnd);
        }

        internal static string GetUnininstallerPath()
        {
            return XML.GetPath.GetUninstallerPath();
        }

        public static string GetUNCPath(string filePath)
        {
            if (filePath.StartsWith(@"\\"))
                return filePath;

            if (new DriveInfo(Path.GetPathRoot(filePath)).DriveType != DriveType.Network)
                return filePath;

            string drivePrefix = Path.GetPathRoot(filePath).Substring(0, 2);
            string uncRoot;
            using (var managementObject = new ManagementObject())
            {
                var managementPath = string.Format("Win32_LogicalDisk='{0}'", drivePrefix);
                managementObject.Path = new ManagementPath(managementPath);
                uncRoot = (string)managementObject["ProviderName"];
            }
            return filePath.Replace(drivePrefix, uncRoot);
        }

        internal static string GetInstallerFilesDirectory(DTO.Enums.BackEndOrFrontEndEnum whichEnd)
        {
            return GetRolloutDirectoryPath(whichEnd) + "Front End Installer Files\\";
        }

        internal static string GetInstallerFilesDirectory(string rolloutDirectory)
        {
            return rolloutDirectory + "Front End Installer Files\\";
        }
    }
}
