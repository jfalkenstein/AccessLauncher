using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AccessLauncher.XML
{
    /* This class is exclusively for the creation of the various XML documents used throughout this
     * application. For explanation on the various methods, see the comments on each one.
     */
    
    public class CreateXmlDoc
    {
        //This function creates the BackEndSettings.xml file with the passed in RolloutDirectory.
        //This function is only called through the back end installation process.
        public static void CreateBackEndSettingsXMLFile(string rolloutDirectory, string connectionString)
        {
            string currentDirectory = Directory.GetCurrentDirectory() + "\\";
            XmlDocument doc = new XmlDocument();

            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            XmlElement backEndSettings = doc.CreateElement("BackEndSettings");
            doc.AppendChild(backEndSettings);

            XmlElement installedDirectory = doc.CreateElement("InstalledDirectory");
            installedDirectory.AppendChild(doc.CreateTextNode(currentDirectory));
            backEndSettings.AppendChild(installedDirectory);

            XmlElement connectionStringElement = doc.CreateElement("ConnectionString");
            connectionStringElement.AppendChild(doc.CreateTextNode(connectionString));
            backEndSettings.AppendChild(connectionStringElement);

            XmlElement rolloutDirectoryElement = doc.CreateElement("RolloutDirectory");
            rolloutDirectoryElement.AppendChild(doc.CreateTextNode(rolloutDirectory));
            backEndSettings.AppendChild(rolloutDirectoryElement);

            SaveXmlDoc.saveBackEndSettingsDoc(doc);
        }

        //This creates an empty backend.xml file with 
        public static void CreateBackEndXmlFileInRolloutDirectory()
        {
            //Creates the back end XML file that users will access. It is mostly empty except with some basic structure 
            //and the current version number is set to 0.
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            XmlElement BackEnd = doc.CreateElement("BackEnd");
            doc.AppendChild(BackEnd);

            XmlElement currentVersionsElement = doc.CreateElement("CurrentVersions");
            foreach (string name in Enum.GetNames(typeof(DTO.Enums.UserTypeEnum)))
            {
                currentVersionsElement.SetAttribute(name, "0"); ;
            }
            currentVersionsElement.SetAttribute("LatestDate", "");
            BackEnd.AppendChild(currentVersionsElement);

            XmlElement lockoutElement = doc.CreateElement("Lockout");
            lockoutElement.InnerText = false.ToString();
            BackEnd.AppendChild(lockoutElement);

            XmlElement rolloutsElement = doc.CreateElement("RollOuts");
            BackEnd.AppendChild(rolloutsElement);
            string connectionString = GetData.GetCurrentConnectionString(DTO.Enums.BackEndOrFrontEndEnum.BackEnd);
            
            XmlElement connectionInfoElement = doc.CreateElement("ConnectionInfo");
            connectionInfoElement.SetAttribute("ConnectionString",connectionString);
            connectionInfoElement.SetAttribute("ConnectionDateSet", DateTime.Now.ToString());
            BackEnd.AppendChild(connectionInfoElement);

            SaveXmlDoc.saveBackEndDoc(doc);
        }

        //This method is ultimately called by FrontEndInstaller application. It creates the frontend.xml file
        //In the user's appDataDirectory. The Xml nodes are mostly empty except for the rollout directory and the 
        //Uninstaller location. These are pre-populated because they are necessary from the start for 
        //the front-end to operate.
        public static void CreateFrontEndXMLFile(string rolloutDirectory, string appDataDirectory, int currentLauncherVersion)
        {
            var doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);

            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            XmlElement baseDoc = doc.CreateElement("FrontEndSettings");
            doc.AppendChild(baseDoc);

            XmlElement launcherVersion = doc.CreateElement("LauncherVersion");
            launcherVersion.InnerText = currentLauncherVersion.ToString();
            baseDoc.AppendChild(launcherVersion);

            XmlElement rolloutDirectoryEl = doc.CreateElement("RolloutDirectory");
            rolloutDirectoryEl.InnerText = rolloutDirectory;
            baseDoc.AppendChild(rolloutDirectoryEl);

            XmlElement uninstallLocation = doc.CreateElement("UninstallerLocation");
            uninstallLocation.InnerText = rolloutDirectory + "AccessLauncher.FeUninstaller.exe";
            baseDoc.AppendChild(uninstallLocation);

            XmlElement currentZipPath = doc.CreateElement("CurrentZipPath");
            baseDoc.AppendChild(currentZipPath);

            XmlElement launchFile = doc.CreateElement("LaunchFile");
            baseDoc.AppendChild(launchFile);

            XmlElement currentUserType = doc.CreateElement("CurrentUserType");
            baseDoc.AppendChild(currentUserType);

            XmlElement currentVersion = doc.CreateElement("CurrentVersion");
            baseDoc.AppendChild(currentVersion);

            SaveXmlDoc.saveFrontEndDoc(doc);

        }

        //This creates a manifest file whenever the back end installs. This manifest file is used by the
        //Front end installer to know what files to copy.
        public static void CreateManifestXMLFile(string locationToPlaceManifest, List<DTO.InstallerItem> installerManifest)
        {
            var doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);

            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            XmlElement topElement = doc.CreateElement("Base");
            doc.AppendChild(topElement);


            XmlElement launcherVersion = doc.CreateElement("LauncherVersion");
            launcherVersion.InnerText = "1";
            topElement.AppendChild(launcherVersion);

            XmlElement baseDoc = doc.CreateElement("InstallerManifest");
            topElement.AppendChild(baseDoc);

            //Add each item in the manifest to the XML document, stripping out the full path
            foreach (var item in installerManifest)
            {
                FileInfo file = new FileInfo(item.DestinationFilePath);
                XmlElement fileElement = doc.CreateElement("File");
                string fileName = item.DestinationFilePath.Replace(locationToPlaceManifest, "");
                fileElement.SetAttribute("RelativePath", file.Name);
                fileElement.SetAttribute("FullPath", file.FullName);
                baseDoc.AppendChild(fileElement);
            }
            SaveXmlDoc.saveManifestDoc(locationToPlaceManifest, doc);
        }

        //This creates a backup rollout xml document that is placed within the zip file being rolled out.
        //This backup can be used if the back end ever gets broken to restore the rollout file.
        internal static void createBackupRolloutDocument(XmlNode rolloutNode, int versionNumber, string connectionString)
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);

            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            XmlElement baseDoc = doc.CreateElement("RolloutDocumentation");
            doc.AppendChild(baseDoc);

            XmlElement connInfoElement = doc.CreateElement("ConnectionInfo");
            connInfoElement.SetAttribute("ConnectionString", connectionString);
            connInfoElement.SetAttribute("ConnectionDateSet", DateTime.Now.ToString());
            baseDoc.AppendChild(connInfoElement);

            string zipPath = rolloutNode.Attributes.GetNamedItem("FullZipPath").Value;
            DateTime rolloutDate = DateTime.Parse(rolloutNode.Attributes.GetNamedItem("DateTimeStamp").Value);

            XmlElement version = doc.CreateElement("Version");
            version.InnerText = versionNumber.ToString();
            baseDoc.AppendChild(version);

            XmlElement userTypeEl = doc.CreateElement("UserType");
            userTypeEl.InnerText = rolloutNode.Name;
            baseDoc.AppendChild(userTypeEl);

            foreach (XmlAttribute att in rolloutNode.Attributes)
            {
                XmlElement newElement = doc.CreateElement(att.Name);
                newElement.InnerText = att.Value;
                baseDoc.AppendChild(newElement);
            }

            string backUpXML = Directory.GetCurrentDirectory() + "\\TempBackups\\" + rolloutDate.ToShortDateString().Replace("/", "-") + ".xml";

            doc.Save(backUpXML);
            using (Ionic.Zip.ZipFile zip = Ionic.Zip.ZipFile.Read(zipPath))
            {
                try
                {
                    var file = zip.Entries.Single(p => p.FileName.Contains(".xml"));
                    if (file != null) zip.RemoveEntry(file.FileName);
                    zip.Save();
                }
                catch (Exception)
                {
                }
                zip.AddFile(backUpXML, "");
                zip.Save();
                zip.Dispose();
            }

        }

    }
}
