using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AccessLauncher.XML
{
    /* The purpose of this class is to obtain the various xml documents used throughout this application. It is only
     * used within the XML layer, and for most of the methods, it will make use of caching to minimize file system read/write
     * cycles.
     */
    
    internal class GetXmlDoc
    {
        //This method will return the back end xml document.
        internal static XmlDocument GetBackEndXmlDoc(DTO.Enums.BackEndOrFrontEndEnum whichEnd)
        {
            var doc = new XmlDocument();
            try
            {
                doc.Load(GetPath.GetBackEndXMLPath(whichEnd));
            }
            catch (DirectoryNotFoundException)
            {
                throw new DTO.Exceptions.CouldNotLocateRolloutDirectoryException();
            }
            catch (FileNotFoundException)
            {
                throw new DTO.Exceptions.BackEndNotFoundException();
            }
            catch (Exception)
            {
                throw;
            }
            return doc;
        }

        //This function will pull the frontend.xml file loaded into a XmlDocument object from the memory cache.
        internal static XmlDocument GetFrontEndXmlDoc()
        {
            return Caching.PullSettingsDocFromCache(DTO.Enums.BackEndOrFrontEndEnum.FrontEnd);
        }

        //This function will pull the specified settings file desired from the memory cash.
        internal static XmlDocument GetSettingsDoc(DTO.Enums.BackEndOrFrontEndEnum whichEnd)
        {
            return Caching.PullSettingsDocFromCache(whichEnd);
        }
    }
}
