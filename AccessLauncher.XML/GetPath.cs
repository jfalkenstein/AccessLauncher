using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AccessLauncher.XML
{
    /* The purpose of this class is to obtain the various file/folder paths used throughout this application
     * from the various xml documents in which they are stored.
     */
    
    public class GetPath
    {
        //This will locate the rollout directory. This is a key method in all forms of this application.
        //If the rollout directory doesn't exist or if it cannot locate the rollout directory, this will
        //raise an exception, which can be evaluated higher up in the call chain.
        public static string GetRolloutDirectoryPath(DTO.Enums.BackEndOrFrontEndEnum whichEnd)
        {
            try
            {
                var doc = GetXmlDoc.GetSettingsDoc(whichEnd);
                string rolloutDirectoryPath = doc.SelectSingleNode("//RolloutDirectory").InnerText;
                if (rolloutDirectoryPath.Length == 0 || String.IsNullOrEmpty(rolloutDirectoryPath))
                    throw new DTO.Exceptions.CouldNotLocateRolloutDirectoryException();
                return rolloutDirectoryPath;
            }
            catch (Exception)
            {
                if (!File.Exists(GetSettingsXMLPath(whichEnd)))
                {
                    throw new DTO.Exceptions.BackEndSettingsNotFoundException();
                }
                else throw;
            }
        }

        //This function is a simple switch on the BackEndOrFrontEndEnum, providing the relevant settings xml path.
        public static string GetSettingsXMLPath(DTO.Enums.BackEndOrFrontEndEnum whichSettings)
        {
            string path = "";
            switch (whichSettings)
            {
                case AccessLauncher.DTO.Enums.BackEndOrFrontEndEnum.BackEnd:
                    path = Directory.GetCurrentDirectory() + "\\BackEndSettings.xml";
                    break;
                case AccessLauncher.DTO.Enums.BackEndOrFrontEndEnum.FrontEnd:
                    path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PBUSA\\Access Launcher\\frontend.xml";
                    break;
            }
            return path;
        }
        
        //This function provides the path to the backend.xml. This is a key method in all forms of this application.
        public static string GetBackEndXMLPath(DTO.Enums.BackEndOrFrontEndEnum whichEnd)
        {
            return GetRolloutDirectoryPath(whichEnd) + "backend.xml";
        }

        //This function will return just the uninstaller path from the front end settings.
        public static string GetUninstallerPath()
        {
            var doc = GetXmlDoc.GetFrontEndXmlDoc();
            return doc.SelectSingleNode("//UninstallerLocation").InnerText;
        }
    }
}
