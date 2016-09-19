using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Caching;
using System.Xml;
using System.IO;

namespace AccessLauncher.XML
{
    /* The caching class is only used within the XML layer. It functions to minimize the amount of times XML files
     * Need to be accessed on the hard drive. When one of the internal methods on this class are called, they will
     * attempt to pull the specified object from the cache. If it doesn't exist in the cache, it will put it there
     * and try again. This means we can read from memory more often than we read from the hard disk.
     * 
     * There are two internal methods (i.e. accessible only from within the XML namespace): PullSettingsFromCache and
     * PullLatestRolloutFromCache.
     * 
     * -PullSettingsFromCache will pull (and add if it isn't there) the xml string of the BackEndSettings.xml or the
     *  fontend.xml file, depending on the UserType enum passed in. In practice, it is only used for the BackEndSettings
     *  .xml file because PullFrontEndXmlDocFromCache is more effectively used for the the front end.
     *  
     * -PullLatestRolloutFromCache will pull (and add if it isn't there) the pertinent rollout details of the latest
     *  rollout, and communicate it through a DTO.RolloutInfo object.
     */

    class Caching
    {
        internal static XmlDocument PullSettingsDocFromCache(DTO.Enums.BackEndOrFrontEndEnum whichEnd)
        {
            ObjectCache cache = MemoryCache.Default;
            TryAgain:
            XmlDocument doc = cache[Enum.GetName(typeof(DTO.Enums.BackEndOrFrontEndEnum),whichEnd) + "SettingsDoc"] as XmlDocument;
            if (doc == null)
            {
                addSettingsDocToCache(cache,whichEnd);
                goto TryAgain;
            }
            return doc;
        }

        private static void addSettingsDocToCache(ObjectCache cache, DTO.Enums.BackEndOrFrontEndEnum whichEnd)
        {
            CacheItemPolicy policy = createCacheItemPolicy(GetPath.GetSettingsXMLPath(whichEnd));
            var doc = new XmlDocument();
            try
            {
                doc.Load(GetPath.GetSettingsXMLPath(whichEnd));
            }
            catch (FileNotFoundException)
            {
                System.Exception newException = null;
                switch (whichEnd)
	            {
                    case DTO.Enums.BackEndOrFrontEndEnum.BackEnd:
                        newException = new DTO.Exceptions.BackEndSettingsNotFoundException();
                        break;
                    case DTO.Enums.BackEndOrFrontEndEnum.FrontEnd:
                        newException = new DTO.Exceptions.FrontEndSettingsNotFoundException();
                        break;
	            }
                throw newException;
            }
            catch (Exception)
            {
                throw;
            }
            cache.Set(Enum.GetName(typeof(DTO.Enums.BackEndOrFrontEndEnum), whichEnd) + "SettingsDoc", doc, policy);
        }

        private static CacheItemPolicy createCacheItemPolicy(string fileToWatch)
        {
            CacheItemPolicy policy = new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromSeconds(10) };
            policy.ChangeMonitors.Add(new HostFileChangeMonitor(new List<string>() { fileToWatch }));
            return policy;
        }

        internal static DTO.RolloutInfo PullLatestRolloutFromCache(DTO.Enums.UserTypeEnum userType, DTO.Enums.BackEndOrFrontEndEnum whichEnd)
        {

            ObjectCache cache = MemoryCache.Default;
            TryAgain:
            DTO.RolloutInfo rollout = cache["LatestRollout"] as DTO.RolloutInfo;
            try
            {
                if (rollout == null)
                {
                    addLatestRolloutToCache(cache, userType, whichEnd);
                    goto TryAgain;
                }
            }
            catch (Exception)
            {
                
                throw;
            }
            
            return rollout;
            
        }

        private static void addLatestRolloutToCache(ObjectCache cache, DTO.Enums.UserTypeEnum userType, DTO.Enums.BackEndOrFrontEndEnum whichEnd)
        {
            CacheItemPolicy policy = new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromSeconds(10) };
            var rollout = new DTO.RolloutInfo();
            rollout.UserType = userType;
            var doc = GetXmlDoc.GetBackEndXmlDoc(DTO.Enums.BackEndOrFrontEndEnum.FrontEnd);
            string currentVersion = doc.SelectSingleNode("//CurrentVersions").Attributes.GetNamedItem(rollout.UserTypeName).Value;
            rollout.Connection.ConnectionString = GetData.GetCurrentConnectionString(whichEnd);
            rollout.Connection.DateSet = DateTime.Parse(doc.SelectSingleNode("//ConnectionInfo").Attributes.GetNamedItem("ConnectionDateSet").Value);
            rollout.DateTimeStamp = rollout.Connection.DateSet.Value;
            XmlNode rolloutNode = doc.SelectSingleNode("//Version[@value='" + currentVersion + "']/" + Enum.GetName(typeof(DTO.Enums.UserTypeEnum), userType));
            try
            {
                string launchFile = rolloutNode.Attributes.GetNamedItem("LaunchFile").Value;
                rollout.LaunchFile = launchFile;
                string zipPath = rolloutNode.Attributes.GetNamedItem("FullZipPath").Value;
                rollout.ZipPath = zipPath;
            }
            catch (NullReferenceException)
            {
                throw new DTO.Exceptions.CouldNotFindValueException();
            }
            rollout.RolloutVersionString = currentVersion;
            
            cache.Set("LatestRollout", rollout, policy);
        }

    }
}
