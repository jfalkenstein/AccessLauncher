using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.IO.Compression;
using AccessLauncher.Domain.XmlAccess;

namespace AccessLauncher.Domain.BackEnd
{
    /* This is the main class used by the Back End -- and the most important one at that. To construct a rollout, you need:
     *      1. The directory path (i.e. the path the that will be zipped up and rolled out)
     *      2. The Launch path (i.e. the path to the file be launched when the user double clicks their launcher). This will be
     *          shortened to just a file name upon construction.
     *      3. The enumerated userType (at this time, the two options are Admin and Associate, but that could change later, if desired).
     *      --All directory paths will be validated to ensure they exist and that they end with a "/".
     *      --It will automatically get the current version number for the specified user type and then increment it by one.
     *  
     * There is only one public method for this class: Execute. This method will zip up the directory path and place it in the rollout
     * directory. Then it will add a rollout record to the backend.xml file.
     * 
     * The DTO.RolloutInfo class object is the input parameter on the XMLManager.AddRolloutRecord. RolloutInfo is a plain old object,
     * without any methods. It is simply a container for carrying around the relevant information about a rollout between layers of this
     * application, both on the front end and back end.
     * 
     */
    public class Rollout
    {
        public string DirectoryPath { get; set; }
        public string RollOutDirectoryPath { get; private set; }
        public string BackEndXMLPath { get; private set; }
        public string LaunchFileName { get; private set; }
        public int VersionNumber { get; private set; }
        public DTO.Enums.UserTypeEnum UserType { get; set; }


        public Rollout(string directoryPath, string launchPath, DTO.Enums.UserTypeEnum userType)
        {
            //This will ensure that the directory doesn't use mapped drive letters but instead the
            //full UNC path.
            directoryPath = PathManager.GetUNCPath(directoryPath);
            this.LaunchFileName = getLaunchFileName(launchPath);
            this.DirectoryPath = Validator.ValidateDirectoryPath(directoryPath);
            this.UserType = userType;
            this.RollOutDirectoryPath = Validator.ValidateDirectoryPath(PathManager.GetRolloutDirectoryPath(DTO.Enums.BackEndOrFrontEndEnum.BackEnd));
            this.BackEndXMLPath = this.RollOutDirectoryPath + "backend.xml";
            this.VersionNumber = GetDataFromXml.GetCurrentRolloutVersionNumber(DTO.Enums.BackEndOrFrontEndEnum.BackEnd, this.UserType) + 1;
        }

        private string getLaunchFileName(string launchPath)
        {
            FileInfo file = new FileInfo(launchPath);
            return file.Name;
        }

        public void Execute()
        {
            string zipPath = zipUpPath(this.DirectoryPath);
            UpdateXmlFile.AddRolloutRecord(makeRolloutInfo(zipPath));
        }
        //This method will return the new zipfile's path after it zips up. 
        private string zipUpPath(string directoryPathToZip)
        {
            string destinationZipFullName = this.RollOutDirectoryPath + "Rollouts\\" 
                + makeZipFileName();
            TryAgain:
            try
            {
                ZipFile.CreateFromDirectory(this.DirectoryPath, destinationZipFullName);
            }
            catch (System.IO.IOException ex)
            {
                //If the zipfile already exists, delete it and try it again.
                if(ex.Message.Contains("already exists."))
                {
                    File.Delete(destinationZipFullName);
                    goto TryAgain;
                }
                //If a file within the directory to be zipped is currently being used, strip from the error message
                //the name of the file being used, and pass it up as a FileToZipWasOpenException, enabling the user
                //to close it and then try again.
                else if(ex.Message.Contains("because it is being used by another process."))
                {
                    int startPostion = ex.Message.IndexOf("'")+1;
                    int endPosition = ex.Message.LastIndexOf("'")-1;
                    int length = endPosition-startPostion+1;
                    string problemFile = ex.Message.Substring(startPostion, length);
                    throw new DTO.Exceptions.FileToZipWasOpenException(problemFile);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return destinationZipFullName;
        }

        private string makeZipFileName()
        {
            string user = Enum.GetName(typeof(DTO.Enums.UserTypeEnum), this.UserType);
            return "FE-" + this.VersionNumber.ToString() + "-" + user + ".zip";
        }
        //This method will return a DTO.RolloutInfo out of this class's properties, adding a date stamp of now.
        private DTO.RolloutInfo makeRolloutInfo(string zipPath)
        {
            return new DTO.RolloutInfo() { UserType = this.UserType, 
                LaunchFile = this.LaunchFileName, 
                RolloutDirectory = this.RollOutDirectoryPath, 
                ZipPath = zipPath, 
                DateTimeStamp = DateTime.Now, 
                RolloutVersionNumber = this.VersionNumber,
                Connection = new DTO.RolloutInfo.ConnectionInfo() 
                    { ConnectionString = GetDataFromXml.GetCurrentConnectionString(DTO.Enums.BackEndOrFrontEndEnum.BackEnd),
                      DateSet = DateTime.Now}
            };
        }

    }


}
