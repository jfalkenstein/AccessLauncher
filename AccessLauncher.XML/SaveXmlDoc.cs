using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace AccessLauncher.XML
{
    internal class SaveXmlDoc
    {
        //This is special method used for saving the BackEndSettings.xml doc, which will raise a special custom
        //exception if it cannot.
        internal static void saveBackEndSettingsDoc(XmlDocument Doc)
        {
            saveDoc(GetPath.GetSettingsXMLPath(DTO.Enums.BackEndOrFrontEndEnum.BackEnd), 
                    new DTO.Exceptions.CouldNotSaveBackEndSettingsException(),
                    Doc);
        }

        //This is a special method used for saving the backend.xml doc, which will raise a special custom
        //exception if it cannot.
        internal static void saveBackEndDoc(XmlDocument Doc)
        {
            saveDoc(GetPath.GetBackEndXMLPath(DTO.Enums.BackEndOrFrontEndEnum.BackEnd), 
                    new DTO.Exceptions.CouldNotSaveBackEndXmlException(), 
                    Doc);
        }

        //This is a special method used for saving the Front End Doc, which will raise a special custom 
        //exception if it cannot.
        internal static void saveFrontEndDoc(XmlDocument doc)
        {
            saveDoc(GetPath.GetSettingsXMLPath(DTO.Enums.BackEndOrFrontEndEnum.FrontEnd), 
                    new DTO.Exceptions.CouldNotSaveFrontEndXmlDocException(),
                    doc);
        }
        //This is a special method used for saving the installer manifest doc, which will raise a special
        //custom exception if it cannot. --Overload: Will take an XmlDocument as a parameter
        internal static void saveManifestDoc(string locationToPlaceManifest, XmlDocument doc)
        {
            string xmlFilePath = locationToPlaceManifest + "InstallerManifest.xml";
            saveDoc(xmlFilePath, 
                    new DTO.Exceptions.CouldNotSaveManifestXmlDocException(),
                    doc);
        }

        //This is a special method used for saving the installer manifest doc, which will raise a special
        //custom exception if it cannot. --Overload: Will take an XDocument as a parameter
        internal static void saveManifestDoc(string locationToPlaceManifest, XDocument doc)
        {
            string xmlFilePath = locationToPlaceManifest + "InstallerManifest.xml";
            saveDoc(xmlFilePath,
                    new DTO.Exceptions.CouldNotSaveManifestXmlDocException(),
                    doc);
        }

        //This method will attempt to save the passed in document (regardless of whether it is an XDocument or XmlDocument) and
        //will throw an exception if it cannot.
        private static void saveDoc(string filePath, DTO.AbstractClasses.XmlSaveException exceptionToRaise, dynamic doc)
        {
            if(doc.GetType().Name != typeof(XDocument).Name && doc.GetType().Name != typeof(XmlDocument).Name)
            {
                throw new InvalidDataException("You cannot use SaveDoc without passing in either an XmlDocument or an XDocument");
            }
            try
            {
                doc.Save(filePath);
            }
            catch(XmlException)
            {
                exceptionToRaise.FilePathThatCouldNotSave = filePath;
                throw exceptionToRaise;
            }
            catch(Exception)
            {
                throw;
            }

        }
    }
}
