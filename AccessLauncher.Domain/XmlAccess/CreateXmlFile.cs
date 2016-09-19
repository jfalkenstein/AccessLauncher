using AccessLauncher.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessLauncher.Domain.XmlAccess
{
    /* The purpose of this class is to provide access to the XML file creation methods within the XML layer.
     * For explanation on these methods, see their called method in XML.CreateXmlDoc.
     */
    
    public class CreateXmlFile
    {
        public static void CreateBackEndSettingsXMLFile(string rolloutDirectory, string connectionString)
        {
            XML.CreateXmlDoc.CreateBackEndSettingsXMLFile(rolloutDirectory, connectionString);
        }

        public static void CreateBackEndXmlFileInRolloutDirectory()
        {
            XML.CreateXmlDoc.CreateBackEndXmlFileInRolloutDirectory();
        }

        public static void CreateFrontEndXMLFile(string rolloutDirectory, string appDataDirectory, int currentLauncherVersion)
        {
            XML.CreateXmlDoc.CreateFrontEndXMLFile(rolloutDirectory, appDataDirectory, currentLauncherVersion);
        }

        internal static void CreateManifestXMLFile(string locationToPlaceManifest, List<InstallerItem> installerManifest)
        {
            XML.CreateXmlDoc.CreateManifestXMLFile(locationToPlaceManifest, installerManifest);
        }
    }
}
