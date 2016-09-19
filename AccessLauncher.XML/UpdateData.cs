using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.IO.Compression;
using Ionic.Zip;


namespace AccessLauncher.XML
{
    /* The UpdateData class is composed of static methods whose purpose is to write into the various XML files.
     * 
     * See comments on individual methods for explanations.
     */
    public class UpdateData
    {
        //This is a main function in the Back End tool. It takes a RolloutInfo object and with that info,
        //it creates a new rollout record.
        public static void AddRolloutRecord(DTO.RolloutInfo rollout)
        {
            var doc = GetXmlDoc.GetBackEndXmlDoc(DTO.Enums.BackEndOrFrontEndEnum.BackEnd);
            //Compare current connection string and date and resolve for latest string
            string latestConnectionString = compareConnectionStrings(ref doc, rollout);
            XmlNode versionNode = doc.SelectSingleNode("//Version[@value='"+ rollout.RolloutVersionString +"']");
            //Check to see if a node for the current version exists
            if(versionNode != null)
            {
                //If a node for the current version exists, then check if a rollout node exists for the specified userType
                XmlNode rolloutNode = versionNode.SelectSingleNode("./" + rollout.UserTypeName);
                if(rolloutNode != null)
                {
                    //If a rollout node exists for the userType, then modify the values with the most up-to-date info.
                    rolloutNode = setRolloutAttributes(doc, rolloutNode, rollout);
                }
                else
                {
                    //If a rollout node does NOT exist for the userType, then create one.
                    versionNode.AppendChild(createRolloutNode(doc, rollout));
                }
            }
            //If a version node for the current version number does NOT exist, then create one with attributes set
            else
            {
                XmlNode rolloutsNode = doc.SelectSingleNode("//BackEnd/RollOuts");
                XmlElement newVersionNode = doc.CreateElement("Version");
                newVersionNode.SetAttribute("value", rollout.RolloutVersionString);
                newVersionNode.AppendChild(createRolloutNode(doc, rollout));
                rolloutsNode.AppendChild(newVersionNode);
            }
            //Compare current version number with highest listed version number. If current is less than highest, update current.
            ensureHighestVersionNumber(doc, rollout.UserTypeName);
            ensureLatestRolloutDate(doc, rollout.DateTimeStamp);
            //Create backup rollout document within zip folder
            XmlNode newRolloutNode = doc.SelectSingleNode("//Version[@value='" + rollout.RolloutVersionString + "']").SelectSingleNode("./" + rollout.UserTypeName);
            CreateXmlDoc.createBackupRolloutDocument(newRolloutNode, rollout.RolloutVersionNumber,latestConnectionString);
            SaveXmlDoc.saveBackEndDoc(doc);
        }

        private static string compareConnectionStrings(ref XmlDocument doc, DTO.RolloutInfo rollout)
        {
            var connNode = doc.SelectSingleNode("//ConnectionInfo");
            string connString = "";
            if (connNode != null && !string.IsNullOrEmpty(rollout.Connection.ConnectionString) && rollout.Connection.DateSet != null)
            {
                try
                {
                    string currentString = connNode.Attributes.GetNamedItem("ConnectionString").Value;
                    DateTime currentStringDate = DateTime.Parse(connNode.Attributes.GetNamedItem("ConnectionDateSet").Value);
                    if (DateTime.Compare(currentStringDate, rollout.Connection.DateSet.Value) < 0 || String.IsNullOrEmpty(currentString))
                    {
                        connString = connNode.Attributes.GetNamedItem("ConnectionString").Value = rollout.Connection.ConnectionString;
                        connNode.Attributes.GetNamedItem("ConnectionDateSet").Value = rollout.Connection.DateSet.ToString();
                    }
                    else connString = currentString;
                }
                catch (Exception)
                {
                    connString = connNode.Attributes.GetNamedItem("ConnectionString").Value = rollout.Connection.ConnectionString;
                    connNode.Attributes.GetNamedItem("ConnectionDateSet").Value = rollout.Connection.DateSet.ToString();
                }

            }
            else connString = "";
            return connString;
        }
        
        //This will compare version numbers and make sure the highest one in the document is listed as
        //The current version for the specified UserType.
        private static void ensureHighestVersionNumber(XmlDocument doc, string userTypeName)
        {
            DTO.Enums.UserTypeEnum userType = (DTO.Enums.UserTypeEnum)Enum.Parse(typeof(DTO.Enums.UserTypeEnum), userTypeName);
            int highestVersionNumber = GetData.GetHighestVersionNumber(doc);
            if (highestVersionNumber > GetData.GetCurrentRolloutVersionNumber(doc,userType))
            {
                setCurrentVersionNumber(doc, highestVersionNumber,userType);
            }
        }
        //This will set the current version number.
        private static void setCurrentVersionNumber(XmlDocument doc, int versionNumber, DTO.Enums.UserTypeEnum userType)
        {
            string userTypeName = Enum.GetName(typeof(DTO.Enums.UserTypeEnum), userType);
            doc.SelectSingleNode("//CurrentVersions").Attributes.GetNamedItem(userTypeName).Value = versionNumber.ToString();
        }
        //This will compare rollout dates and make sure the latest one in the document is listed as
        //the latest rollout date.
        private static void ensureLatestRolloutDate(XmlDocument doc, DateTime date)
        {
            DateTime latestDate = GetData.GetLatestRolloutDate(doc);
            if (DateTime.Compare(date,latestDate) > 0 )
            {
                doc.SelectSingleNode("//CurrentVersions").Attributes.GetNamedItem("LatestDate").Value = latestDate.ToString();
            }
        }
        //This creates and returns a rollout node after setting rollout attributes.
        private static XmlNode createRolloutNode(XmlDocument doc, DTO.RolloutInfo rollout)
        {
            XmlElement newNode = doc.CreateElement(rollout.UserTypeName);
            return setRolloutAttributes(doc, newNode, rollout);
        }
        //This will set the necessary rollout node attributes with the neccesary info from the RolloutInfo object
        //Passed into it.
        private static XmlNode setRolloutAttributes(XmlDocument doc, XmlNode node, DTO.RolloutInfo rollout)
        {
            XmlAttribute datestamp = doc.CreateAttribute("DateTimeStamp");
            datestamp.Value = rollout.DateTimeStamp.ToString();
            XmlAttribute zipPath = doc.CreateAttribute("FullZipPath");
            zipPath.Value = rollout.ZipPath;
            XmlAttribute launch = doc.CreateAttribute("LaunchFile");
            launch.Value = rollout.LaunchFile;
            node.Attributes.Append(launch);
            node.Attributes.Append(datestamp);
            node.Attributes.Append(zipPath);
            return node;
        }
    
        /*This method is used to roll back the backend.xml file to a previous version.
         * 1. It will set a the current version to the number passed in for the specified UserType.
         * 2. It will remove all newer versions from the document of the specified UserType.
         * 3. It will then save the document.
         */
        public static void RollBackToVersionNumber(int versionToRollTo, DTO.Enums.UserTypeEnum userType)
        {
            var doc = GetXmlDoc.GetBackEndXmlDoc(DTO.Enums.BackEndOrFrontEndEnum.BackEnd);
            setCurrentVersionNumber(doc, versionToRollTo, userType);
            removeAllNewVersions(doc, versionToRollTo, userType);
            SaveXmlDoc.saveBackEndDoc(doc);
        }
        //This will remove all versions newer than the passed in version from the passed in XmlDocument.
        private static void removeAllNewVersions(XmlDocument doc, int versionToRollTo, DTO.Enums.UserTypeEnum userType)
        {
            string userTypeName = Enum.GetName(typeof(DTO.Enums.UserTypeEnum), userType);
            XmlNodeList nodeList = doc.SelectNodes("//Version[@value>" + versionToRollTo.ToString() + "]");
            foreach (XmlNode node in nodeList)
            {
                XmlNode foundnode = node.SelectSingleNode("./" + userTypeName);
                if (foundnode != null) foundnode.ParentNode.RemoveChild(foundnode);
            }
        }

        //This is an important method for the FrontEnd. A DTO.RolloutInfo object is passed in and then this will
        //update the frontend.xml file for the user with that info.
        public static void UpdateFrontEndXml(DTO.RolloutInfo rollout)
        {
            var doc = GetXmlDoc.GetFrontEndXmlDoc();
            doc.SelectSingleNode("//LaunchFile").InnerText = rollout.LaunchFile;
            doc.SelectSingleNode("//CurrentUserType").InnerText = rollout.UserTypeName;
            doc.SelectSingleNode("//CurrentVersion").InnerText = rollout.RolloutVersionString;
            doc.SelectSingleNode("//CurrentZipPath").InnerText = rollout.ZipPath;
            SaveXmlDoc.saveFrontEndDoc(doc);
        }
        //This will set the current Front End launcher version to the passed in int.
        public static void UpdateFrontEndLauncherVersion(int currentLauncherVersion)
        {
            var doc = GetXmlDoc.GetFrontEndXmlDoc();
            doc.SelectSingleNode("//LauncherVersion").InnerText = currentLauncherVersion.ToString();
            SaveXmlDoc.saveFrontEndDoc(doc);
        }
        //This will activate or deactivate a global lockout
        public static void SetLockoutStatus(bool enabled)
        {
            var doc = GetXmlDoc.GetBackEndXmlDoc(DTO.Enums.BackEndOrFrontEndEnum.BackEnd);
            doc.SelectSingleNode("//Lockout").InnerText = enabled.ToString();
            SaveXmlDoc.saveBackEndDoc(doc);
        }
        //This will set the current launcher version within the installer manifest of the passed in installerDirectory
        public static void SetLauncherVersion(int newLauncherVersion, string installerDirectory)
        {
            var doc = XDocument.Load(installerDirectory + "InstallerManifest.xml");
            doc.Descendants().Single(p=> p.Name== "LauncherVersion").Value = newLauncherVersion.ToString();
            SaveXmlDoc.saveManifestDoc(installerDirectory,doc);
        }
        //This will add the passed in file to the installer manifest within the passed in installerDirectory
        public static void AddFileToManifest(string installerDirectory, FileInfo file)
        {
            var doc = XDocument.Load(installerDirectory + "InstallerManifest.xml");
            var sameFiles = doc.Descendants("File").Select(p => new FileInfo(p.Attribute("FullPath").Value)).Where(p => p.Name == file.Name);
            if (sameFiles.Count() == 0)
            {
                doc.Descendants().Single(p=> p.Name== "InstallerManifest").Add(
                    new XElement("File",
                        new XAttribute("RelativePath",file.FullName.Replace(installerDirectory,"")),
                        new XAttribute("FullPath",file.FullName)));
                SaveXmlDoc.saveManifestDoc(installerDirectory, doc);
            }

        }
        //This will set the front end version number to zero, thus requiring that it be updated next launch.
        public static void SetFrontEndVersionToZero()
        {
            var doc = GetXmlDoc.GetFrontEndXmlDoc();
            doc.SelectSingleNode("//CurrentVersion").InnerText = "0";
            doc.Save(GetPath.GetSettingsXMLPath(DTO.Enums.BackEndOrFrontEndEnum.FrontEnd));
        }

        //This will set the current connection string in the backend.xml file.
        public static void UpdateConnectionString(string newConnectionString)
        {
            var backEndDoc = GetXmlDoc.GetBackEndXmlDoc(DTO.Enums.BackEndOrFrontEndEnum.BackEnd);
            var backEndSettingsDoc = GetXmlDoc.GetSettingsDoc(DTO.Enums.BackEndOrFrontEndEnum.BackEnd);
            backEndDoc.SelectSingleNode("//ConnectionInfo").Attributes.GetNamedItem("ConnectionString").Value = newConnectionString;
            backEndSettingsDoc.SelectSingleNode("//ConnectionString").InnerText = newConnectionString;
            SaveXmlDoc.saveBackEndDoc(backEndDoc);
            SaveXmlDoc.saveBackEndSettingsDoc(backEndSettingsDoc);
        }
    }
}
