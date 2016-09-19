using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using AccessLauncher.DTO;
using System.Runtime.Caching;

namespace AccessLauncher.XML
{
    /* The purpose of this class is to obtain information from the various XML documents used in this application.
     * 
     * See comments on methods for explanations.
     */
    
    public class GetData
    {
        //This is an overloaded method. This function will return the current rollout version number for the specified UserType, 
        //from the perspective of whichever end of the application the user is calling from.
        public static int GetCurrentRolloutVersionNumber(DTO.Enums.BackEndOrFrontEndEnum whichEnd, DTO.Enums.UserTypeEnum userType)
        {
            string userTypeName = Enum.GetName(typeof(DTO.Enums.UserTypeEnum), userType);
            return int.Parse(GetXmlDoc.GetBackEndXmlDoc(whichEnd).SelectSingleNode("//CurrentVersions")
                .Attributes.GetNamedItem(userTypeName).Value);
        }
        //This is an overloaded method. This function will return the current rollout version number of the passed in XmlDocument,
        //NOT the current backend.xml file. This is used in the process of adding a new rollout.
        internal static int GetCurrentRolloutVersionNumber(XmlDocument doc, DTO.Enums.UserTypeEnum userType)
        {
            string userTypeName = Enum.GetName(typeof(DTO.Enums.UserTypeEnum), userType);
            return int.Parse(doc.SelectSingleNode("//CurrentVersions")
                .Attributes.GetNamedItem(userTypeName).Value);
        }
        //This will acquire the date of the most recent rollout.
        public static DateTime GetMostRecentRolloutDate(DTO.Enums.BackEndOrFrontEndEnum whichEnd)
        {
            var doc = GetXmlDoc.GetBackEndXmlDoc(whichEnd);
            try
            {
                string dateString = doc.SelectSingleNode("//CurrentVersions").Attributes.GetNamedItem("LatestDate").Value;
                if (string.IsNullOrEmpty(dateString)) throw new DTO.Exceptions.CouldNotFindValueException();
                return DateTime.Parse(dateString);
            }
            catch (Exception)
            {

                throw new DTO.Exceptions.CouldNotFindValueException();
            }
        }
        //This will pull all the values from the frontend.xml file and put them into a DTO.RolloutInfo object.
        public static RolloutInfo GetFrontEndSettings()
        {
            RolloutInfo currentRollout = new RolloutInfo();
            var doc = GetXmlDoc.GetFrontEndXmlDoc();
            try
            {
                currentRollout.RolloutDirectory = doc.SelectSingleNode("//RolloutDirectory").InnerText;
                currentRollout.ZipPath = doc.SelectSingleNode("//CurrentZipPath").InnerText;
                currentRollout.LaunchFile = doc.SelectSingleNode("//LaunchFile").InnerText;
                currentRollout.UserTypeName = doc.SelectSingleNode("//CurrentUserType").InnerText;
                currentRollout.RolloutVersionString = doc.SelectSingleNode("//CurrentVersion").InnerText;
                currentRollout.UninstallerPath = doc.SelectSingleNode("//UninstallerLocation").InnerText;
            }
            catch (Exception)
            {
                throw new DTO.Exceptions.FrontEndNeedsUpdateException();
            }
            return currentRollout;
        }
        //This is used when the Installer is trying to fix a broken back end. It will create a DTO.RolloutInfo object
        //from the reconstruction.xml file created during the reconstruction attempt.
        public static RolloutInfo GetReconstructedInfo(string reconstructionXmlPath)
        {
            RolloutInfo rollout = new RolloutInfo();
            var doc = new XmlDocument();
            doc.Load(reconstructionXmlPath);
            rollout.ZipPath = doc.SelectSingleNode("//FullZipPath").InnerText;
            rollout.RolloutVersionString = doc.SelectSingleNode("//Version").InnerText;
            rollout.UserTypeName = doc.SelectSingleNode("//UserType").InnerText;
            rollout.DateTimeStamp = DateTime.Parse(doc.SelectSingleNode("//DateTimeStamp").InnerText);
            rollout.LaunchFile = doc.SelectSingleNode("//LaunchFile").InnerText;
            try
            {
                rollout.Connection.ConnectionString = doc.SelectSingleNode("//ConnectionInfo").Attributes.GetNamedItem("ConnectionString").Value;
                rollout.Connection.DateSet = DateTime.Parse(doc.SelectSingleNode("//ConnectionInfo").Attributes.GetNamedItem("ConnectionDateSet").Value);
            }
            catch (Exception)
            {
                rollout.Connection.ConnectionString = "";
                rollout.Connection.DateSet = null;
            }
            
            return rollout;
        }
        //This is used to obtain the highest version number of all the rollouts in the passed in XmlDocument.
        internal static int GetHighestVersionNumber(XmlDocument doc)
        {
            var maxVersion = doc.SelectNodes("//BackEnd/RollOuts/Version")
                .Cast<XmlElement>()
                .Max(a => int.Parse(a.Attributes["value"].Value));
            return maxVersion;
        }
        //This is used to get the latest rollout date of all the rollouts in the passed in XmlDocument.
        internal static DateTime GetLatestRolloutDate(XmlDocument doc)
        {
            var latestDateTicks = doc.SelectSingleNode("//BackEnd/RollOuts/Version")
                .Cast<XmlElement>()
                .Max(a => DateTime.Parse(a.Attributes.GetNamedItem("DateTimeStamp").Value).Ticks);
            DateTime date = new DateTime(latestDateTicks);
            return date;
        }
        //This function will return the backend.xml file loaded into a XmlDocument object.
        //This function will pull the latest rollout in a RolloutInfo object from the memory cache.
        public static RolloutInfo GetLatestRollout(DTO.Enums.UserTypeEnum userType, DTO.Enums.BackEndOrFrontEndEnum whichEnd)
        {
            try
            {
                return Caching.PullLatestRolloutFromCache(userType, whichEnd);
            }
            catch (Exception)
            {
                
                throw;
            }            
        }
        //This function will return just the LaunchFile name from the front end settings.
        public static string GetLaunchFileName()
        {
            return GetFrontEndSettings().LaunchFile;
        }
        //This function will return a List<DTO.InstallerItem> generated from the current installer manifest.
        //Passing in a null string for destinationDirectory will leave make the DestinationFilePath a relativePath.
        public static List<InstallerItem> GetInstallerManifest(string installerDirectory, string destinationDirectory)
        {
            var doc = XDocument.Load(installerDirectory + "InstallerManifest.xml");
            var newList = from m in doc.Element("Base").Element("InstallerManifest").Elements()
                          select new DTO.InstallerItem()
                          {
                              SourceFilePath = m.Attribute("FullPath").Value,
                              DestinationFilePath = destinationDirectory + m.Attribute("RelativePath").Value
                          };
            return newList.ToList();
        }
        //This function will return the current rolled out launcher version, looking within the passed in installerDirectory path.
        public static int GetCurrentLauncherVersion(string installerDirectory)
        {
            var doc = XDocument.Load(installerDirectory + "InstallerManifest.xml");
            return int.Parse(doc.Element("Base").Element("LauncherVersion").Value);
        }
        //This will get the current launcher version from the user's local front end.
        public static int GetFrontEndLauncherVersion()
        {
            var doc = GetXmlDoc.GetFrontEndXmlDoc();
            return int.Parse(doc.SelectSingleNode("//LauncherVersion").InnerText);
        }
        //This will return true if a global application lock is enabled. Otherwise, it will return false.
        public static bool LockoutIsEnabled(DTO.Enums.BackEndOrFrontEndEnum whichEnd)
        {
            var doc = GetXmlDoc.GetBackEndXmlDoc(whichEnd);
            return bool.Parse(doc.SelectSingleNode("//Lockout").InnerText);
        }

        public static string GetCurrentConnectionString(DTO.Enums.BackEndOrFrontEndEnum whichEnd)
        {
            XmlDocument doc = null;
            string connString = "";
            switch (whichEnd)
            {
                case AccessLauncher.DTO.Enums.BackEndOrFrontEndEnum.BackEnd:
                    doc = GetXmlDoc.GetSettingsDoc(whichEnd);
                    connString = doc.SelectSingleNode("//ConnectionString").InnerText;
                    break;
                case AccessLauncher.DTO.Enums.BackEndOrFrontEndEnum.FrontEnd:
                    doc = GetXmlDoc.GetBackEndXmlDoc(whichEnd);
                    connString = doc.SelectSingleNode("//ConnectionInfo").Attributes.GetNamedItem("ConnectionString").Value;
                    break;
            }
            return connString;
        }
    }
}
