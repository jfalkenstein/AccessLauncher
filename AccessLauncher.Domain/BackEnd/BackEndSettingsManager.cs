using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using AccessLauncher.Domain.XmlAccess;

namespace AccessLauncher.Domain.BackEnd
{
    public class BackEndSettingsManager
    {   
        public static void RollBackVersionNumber(int versionToRollTo, DTO.Enums.UserTypeEnum userType)
        {
            try 
	        {
	            //This will attempt to roll back to the specified version number.
                UpdateXmlFile.RollBackToVersionNumber(versionToRollTo, userType);
	        }
	        catch (Exception)
	        {
		        //IF an exception is raised, it will delete the current backend.xml and then repair the back end, restoring it
                //to what it was before the rollback attempt. then it will throw a CouldNotRollBackException.
                var pathToAccess = GetDataFromXml.GetAccessPathFromCurrentConnectionString(DTO.Enums.BackEndOrFrontEndEnum.BackEnd);
                File.Delete(PathManager.GetBackEndXmlPath(DTO.Enums.BackEndOrFrontEndEnum.BackEnd));
                var installer = new Installer(PathManager.GetRolloutDirectoryPath(DTO.Enums.BackEndOrFrontEndEnum.BackEnd),pathToAccess);
                installer.TryFixBrokenBackEnd();
                throw new DTO.Exceptions.CouldNotRollBackException();
	        }
        }
        //This will return true or false depending on whether a global lockout has been enabled.
        public static bool LockoutIsEnabled()
        {
            return GetDataFromXml.LockoutIsEnabled(DTO.Enums.BackEndOrFrontEndEnum.BackEnd);
        }
        //This will activate a global lockout.
        public static void SetLockout(bool enabled)
        {
            UpdateXmlFile.SetLockout(enabled);
        }

        public static void UpdateConnectionString(string pathToAccess)
        {
            var connString = UserDbManager.MakeConnectionString(pathToAccess);
            XmlAccess.UpdateXmlFile.UpdateConnectionString(connString);
        }

    }
}
