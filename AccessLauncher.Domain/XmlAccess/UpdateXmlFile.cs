using AccessLauncher.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessLauncher.Domain.XmlAccess
{
    /* The purpose of this class is to provide access to the XML file update methods within the XML layer.
     * For explanation on these methods, see their called method in XML.UpdateData.
     */
    
    internal class UpdateXmlFile
    {
        internal static void AddRolloutRecord(DTO.RolloutInfo rollout)
        {
            XML.UpdateData.AddRolloutRecord(rollout);
        }

        internal static void RollBackToVersionNumber(int versionToRollTo, DTO.Enums.UserTypeEnum userType)
        {
            XML.UpdateData.RollBackToVersionNumber(versionToRollTo, userType);
        }

        internal static void UpdateFrontEndXml(RolloutInfo rollout)
        {
            XML.UpdateData.UpdateFrontEndXml(rollout);
        }

        internal static void UpdateFrontEndLauncherVersion(int currentLauncherVersion)
        {
            XML.UpdateData.UpdateFrontEndLauncherVersion(currentLauncherVersion);
        }

        internal static void SetLockout(bool enabled)
        {
            XML.UpdateData.SetLockoutStatus(enabled);
        }

        internal static void SetLauncherVersion(int newLauncherVersion, string installerDirectory)
        {
            XML.UpdateData.SetLauncherVersion(newLauncherVersion, installerDirectory);
        }

        internal static void AddFileToManifest(string installerDirectory, FileInfo file)
        {
            XML.UpdateData.AddFileToManifest(installerDirectory, file);
        }

        internal static void SetFrontEndVersionToZero()
        {
            XML.UpdateData.SetFrontEndVersionToZero();
        }
        internal static void UpdateConnectionString(string newConnectionString)
        {
            XML.UpdateData.UpdateConnectionString(newConnectionString);
        }
    }
}
