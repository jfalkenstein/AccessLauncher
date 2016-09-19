using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccessLauncher.Domain.XmlAccess;

namespace AccessLauncher.Domain.FrontEnd
{
    /* This class is composed entirely of static methods used for checking whether or not the user's Front End needs
     * update.
     * 
     * There are two public methods: CompareLauncherVersions and CompareUserInfo.
     * 
     * CompareLauncherVersions will:
     *      1. Check whether the currently implemented launcherVersion is the same as the most recently rolled out version.
     *      2. If the version numbers are not the same, then the UpdateLauncher method will be called from the FrontEndUpdater class. 
     *      
     * CompareUserInfo will return a boolean depending on whether the current user's version number and usertype are different than 
     * whatever versionNumber and userType they should have.
     */

    public class UpdateChecker
    {
        public static void CompareLauncherVersions()
        {
            int launcherVersion = GetInfo.GetFrontEndLauncherVersion();
            int currentLauncherVersion = GetInfo.GetCurrentLauncherVersion();
            if (currentLauncherVersion != launcherVersion)
            {
                Console.WriteLine("Updating launcher version...");
                FrontEndUpdater.UpdateLauncher();
            }
        }

        public static bool CompareUserInfo(DTO.User user)
        {
            try
            {
                DTO.RolloutInfo currentRollout = GetDataFromXml.GetFrontEndSettings();
                if (user.UserType != currentRollout.UserType)
                    return true;
                if (currentRollout.RolloutVersionNumber !=
                    GetDataFromXml.GetCurrentRolloutVersionNumber(DTO.Enums.BackEndOrFrontEndEnum.FrontEnd, user.UserType))
                    return true;
            }
            catch (DTO.Exceptions.CouldNotFindValueException)
            {
                throw;
            }
            //This exception is raised when the front end has yet to be set up (i.e. first launch after install).
            catch (DTO.Exceptions.FrontEndNeedsUpdateException)
            {
                return true;
            }
            return false;
        }
    }
}
