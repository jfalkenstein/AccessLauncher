using AccessLauncher.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessLauncher.Domain.XmlAccess
{
    /* The purpose of this class is to provide access to the XML file data retrieval methods within the XML layer.
     * For explanation on these methods, see their called method in XML.GetData.
     */
    
    public class GetDataFromXml
    {
        public static int GetCurrentRolloutVersionNumber(DTO.Enums.BackEndOrFrontEndEnum whichEnd, DTO.Enums.UserTypeEnum userType)
        {
            try
            {
                return XML.GetData.GetCurrentRolloutVersionNumber(whichEnd, userType);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public static DateTime GetMostRecentRolloutDate(DTO.Enums.BackEndOrFrontEndEnum whichEnd)
        {
            return XML.GetData.GetMostRecentRolloutDate(whichEnd);
        }

        internal static RolloutInfo GetReconstructedInfo(string reconstructionXmlPath)
        {
            return XML.GetData.GetReconstructedInfo(reconstructionXmlPath);
        }

        public static RolloutInfo GetFrontEndSettings()
        {
            try
            {
                return XML.GetData.GetFrontEndSettings();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static RolloutInfo GetLatestRollout(DTO.Enums.UserTypeEnum userType, DTO.Enums.BackEndOrFrontEndEnum whichEnd)
        {
            return XML.GetData.GetLatestRollout(userType, whichEnd);
        }

        public static string GetCurrentConnectionString(DTO.Enums.BackEndOrFrontEndEnum whichEnd)
        {
            return XML.GetData.GetCurrentConnectionString(whichEnd);
        }

        public static string GetAccessPathFromCurrentConnectionString(DTO.Enums.BackEndOrFrontEndEnum whichEnd)
        {
            string connString = GetCurrentConnectionString(whichEnd);
            var builder = new System.Data.OleDb.OleDbConnectionStringBuilder(connString);
            string accessPath = builder.DataSource;
            return accessPath;
        }
        
        internal static string GetLaunchFileName()
        {
            return XML.GetData.GetLaunchFileName();
        }

        internal static List<DTO.InstallerItem> GetInstallerManifest(string installerDirectory, string destinationDirectory)
        {
            return XML.GetData.GetInstallerManifest(installerDirectory, destinationDirectory);
        }

        internal static int GetCurrentLauncherVersion(string installerDirectory)
        {
            return XML.GetData.GetCurrentLauncherVersion(installerDirectory);
        }

        internal static int GetFrontEndLauncherVersion()
        {
            return XML.GetData.GetFrontEndLauncherVersion();
        }

        internal static bool LockoutIsEnabled(DTO.Enums.BackEndOrFrontEndEnum whichEnd)
        {
            return XML.GetData.LockoutIsEnabled(whichEnd);
        }
    }
}
