using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using AccessLauncher.Domain.XmlAccess;
using System.ComponentModel;

namespace AccessLauncher.Domain.BackEnd
{
    public class Installer
    {
        /*The installer class is one of the main clases used by the back end. All that is necessary to construct it is
         * a rollout directory.
         * 
         * The installer class has two public methods: Install and TryFixBrokenBackEnd.
         * 
         * Install will do the following:
         *      1. create the needed directory structures for the BackEnd to run
         *      2. Create an installer manifest. This manifest is used by the front end installer to know what files it needs to copy.
         *          This manifest file can be added to as needed. (Removing records is far more rare. It can be manually done, as needed,
         *          but at this time is not programmed. Removing records would essentially eliminate files from the front end--files which could
         *          be necessary, and thus could cause serious problems with the launcher's functionality.
         *      3. Copy necessary files to the rollout directory.
         *      4. Create an installer shortcut, a link that can be sent to anybody with access to the rollout directory.
         *      5. Create both the BackEndSettings.xml and backend.xml files.
         * 
         * See comments on TryFixBrokenBackEnd for a description of its process.         * 
         */
        private string _connectionString;
        public string RollOutDirectory { get; set; }
        public int VersionNumber { get; private set; }
        public string ReconstructionPath { get; private set; }
        public List<DTO.InstallerItem> InstallerManifest { get; set; }

        public Installer(string rollOutDirectory, string pathToAccessBE)
        {
            rollOutDirectory = PathManager.GetUNCPath(rollOutDirectory);
            this.RollOutDirectory = Validator.ValidateDirectoryPath(rollOutDirectory);
            this.ReconstructionPath = this.RollOutDirectory + "Reconstruction.xml";
            _connectionString = UserDbManager.MakeConnectionString(pathToAccessBE);
            VersionNumber = 0;
        }

        public void Install()
        {
            createNecessaryDirectories();
            createManifest();
            copyFilesToRolloutDirectory();
            CreateXmlFile.CreateBackEndSettingsXMLFile(this.RollOutDirectory,_connectionString);
            CreateXmlFile.CreateManifestXMLFile(PathManager.GetInstallerFilesDirectory(this.RollOutDirectory), this.InstallerManifest);
            createInstallerShortcut();
            if(rolloutsExist())
            {
                reconstructBackEndFromRolloutsFolder();
            }
            else
            {
                CreateXmlFile.CreateBackEndXmlFileInRolloutDirectory();
            }
            
        }

        private void createNecessaryDirectories()
        {
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\TempBackups");
            Directory.CreateDirectory(PathManager.GetInstallerFilesDirectory(this.RollOutDirectory) + "\\Images");
            Directory.CreateDirectory(PathManager.GetInstallerFilesDirectory(this.RollOutDirectory) + "\\Launcher Archive");
            Directory.CreateDirectory(this.RollOutDirectory + "Front End Installer Templates");
            Directory.CreateDirectory(this.RollOutDirectory + "Rollouts");
            foreach(string name in Enum.GetNames(typeof(DTO.Enums.UserTypeEnum)))
            {
                Directory.CreateDirectory(this.RollOutDirectory + "Front End Templates\\" + name);
            }
        }
        
        private void createManifest()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string installerDirectory = PathManager.GetInstallerFilesDirectory(this.RollOutDirectory);
            this.InstallerManifest = new List<DTO.InstallerItem>()
            {
                new DTO.InstallerItem() 
                { 
                    SourceFilePath = currentDirectory + "\\AccessLauncher.Domain.dll", 
                    DestinationFilePath = installerDirectory + "AccessLauncher.Domain.dll"
                },
                new DTO.InstallerItem() 
                { 
                    SourceFilePath = currentDirectory + "\\AccessLauncher.DTO.dll", 
                    DestinationFilePath = installerDirectory + "AccessLauncher.DTO.dll"
                },
                new DTO.InstallerItem() 
                { 
                    SourceFilePath = currentDirectory + "\\AccessLauncher.XML.dll", 
                    DestinationFilePath = installerDirectory + "AccessLauncher.XML.dll"
                },
                new DTO.InstallerItem() 
                { 
                    SourceFilePath = currentDirectory + "\\AccessLauncher.FrontEnd.exe", 
                    DestinationFilePath = installerDirectory + "AccessLauncher.FrontEnd.exe"
                },
                new DTO.InstallerItem() 
                { 
                    SourceFilePath = currentDirectory + "\\Images\\p_b_blue.ico", 
                    DestinationFilePath = installerDirectory + "\\Images\\p_b_blue.ico"
                },
                new DTO.InstallerItem() 
                { 
                    SourceFilePath = currentDirectory + "\\AccessLauncher.AccessDBInterface.dll", 
                    DestinationFilePath = installerDirectory + "AccessLauncher.AccessDBInterface.dll"
                }
            };
        }

        private void copyFilesToRolloutDirectory()
        {
            foreach(var item in this.InstallerManifest)
            {
                item.ExecuteCopy();
            }
            File.Copy(Directory.GetCurrentDirectory() + "\\AccessLauncher.FeUninstaller.exe", this.RollOutDirectory + "AccessLauncher.FeUninstaller.exe", true);
            File.Copy(Directory.GetCurrentDirectory() + "\\AccessLauncher.FrontEndInstaller.exe", PathManager.GetInstallerFilesDirectory(this.RollOutDirectory) +"\\AccessLauncher.FrontEndInstaller.exe", true);
        }

        private void createInstallerShortcut()
        {
            string installerDirectory = PathManager.GetInstallerFilesDirectory(this.RollOutDirectory);
            IWshRuntimeLibrary.WshShell wsh = new IWshRuntimeLibrary.WshShell();
            IWshRuntimeLibrary.IWshShortcut shortcut = wsh.CreateShortcut(this.RollOutDirectory + "Front End Installer.lnk");
            shortcut.TargetPath = installerDirectory + "\\AccessLauncher.FrontEndInstaller.exe";
            shortcut.WorkingDirectory = installerDirectory.Substring(0,installerDirectory.Length-1);
            shortcut.Save();
        }

        private bool rolloutsExist()
        {
            var rolloutsFolder = new DirectoryInfo(this.RollOutDirectory + "//Rollouts");
            return rolloutsFolder.EnumerateFiles("FE*").Any();
        }

        /// <summary>
        /// This overload attempts to fix a broken back end making use of a background worker to report progess on the reconstruction
        /// </summary>
        /// <param name="worker"></param>
        /// <returns></returns>
        public void TryFixBrokenBackEnd(ref BackgroundWorker worker)
        {
            worker.WorkerSupportsCancellation = true;
            try
            {
                //1. Check for back end settings file;
                if (!Validator.BackEndSettingsFileExists()) CreateXmlFile.CreateBackEndSettingsXMLFile(this.RollOutDirectory, _connectionString);
                //2. Check for Rollouts folder. If doesn't exist, completely reinstall backend
                if (!Directory.Exists(this.RollOutDirectory + "Rollouts"))
                {
                    createNecessaryDirectories();
                    CreateXmlFile.CreateBackEndXmlFileInRolloutDirectory();
                }
                //3. If folder exists, loop through files in rollouts folder and reconstruct backend xml file.
                else
                {
                    worker.DoWork += reconstructBackEndFromRolloutsFolder;
                    worker.RunWorkerAsync("Reconstructing rollouts...");
                }
            }
            catch (Exception)
            {
                worker.CancelAsync();
            }
        }
        /// <summary>
        /// This overload attempts to fix a broken back end without reporting its progress.
        /// </summary>
        /// <returns></returns>
        public bool TryFixBrokenBackEnd()
        {
            bool success = false;
            try
            {
                //1. Check for back end settings file;
                if (!Validator.BackEndSettingsFileExists()) CreateXmlFile.CreateBackEndSettingsXMLFile(this.RollOutDirectory, _connectionString);
                //2. Check for Rollouts folder. If doesn't exist, completely reinstall backend
                if (!Directory.Exists(this.RollOutDirectory + "Rollouts"))
                {
                    createNecessaryDirectories();
                    CreateXmlFile.CreateBackEndXmlFileInRolloutDirectory();
                }
                //3. If folder exists, loop through files in rollouts folder and reconstruct backend xml file.
                else
                {
                    reconstructBackEndFromRolloutsFolder();
                }

                success = true;
            }
            catch (Exception)
            {
                success = false;
            }
            return success;
        }

        private void reconstructBackEndFromRolloutsFolder(object sender, DoWorkEventArgs e)
        {
            //1. Create back end xml file to be updated.
            CreateXmlFile.CreateBackEndXmlFileInRolloutDirectory();
            var directory = new DirectoryInfo(this.RollOutDirectory + "Rollouts\\");            
            var fileCount = directory.EnumerateFiles("FE-*.zip").Count();
            int progressPercentage = Convert.ToInt32(((double)1 / (fileCount+1)) * 100);
            (sender as BackgroundWorker).ReportProgress(progressPercentage,e.Argument);
            int progress = 1;
            //2. Loop through the zipped files in the rollouts folder
            foreach(var file in directory.EnumerateFiles("FE-*.zip"))
            {
                //For each file, read the file into a ZipArchive object
                ZipArchive zipfile = ZipFile.OpenRead(file.FullName);
                //Find the only .xml file in the zip file and extract it to the reconstruction path.
                zipfile.Entries.FirstOrDefault(p => p.Name.Contains(".xml")).ExtractToFile(this.ReconstructionPath, true);
                //Release the object reference in memory to the zip file.
                zipfile.Dispose();
                //Create a rolloutInfo file (which is plain object with only properties and not methods) out of
                //the newly extracted xml file.
                DTO.RolloutInfo rollout = GetDataFromXml.GetReconstructedInfo(this.ReconstructionPath);         
                //Add a rollout record with the info in the rollout record.
                UpdateXmlFile.AddRolloutRecord(rollout);
                progressPercentage = Convert.ToInt32(((double)++progress / fileCount) * 100);
                (sender as BackgroundWorker).ReportProgress(progressPercentage, "Reconstructed " + file.Name);
            }
        }
        private void reconstructBackEndFromRolloutsFolder()
        {
            //1. Create back end xml file to be updated.
            CreateXmlFile.CreateBackEndXmlFileInRolloutDirectory();
            var directory = new DirectoryInfo(this.RollOutDirectory + "Rollouts\\");
            //2. Loop through the zipped files in the rollouts folder
            foreach (var file in directory.EnumerateFiles("FE-*.zip"))
            {
                //For each file, read the file into a ZipArchive object
                ZipArchive zipfile = ZipFile.OpenRead(file.FullName);
                //Find the only .xml file in the zip file and extract it to the reconstruction path.
                zipfile.Entries.FirstOrDefault(p => p.Name.Contains(".xml")).ExtractToFile(this.ReconstructionPath, true);
                //Release the object reference in memory to the zip file.
                zipfile.Dispose();
                //Create a rolloutInfo file (which is plain object with only properties and not methods) out of
                //the newly extracted xml file.
                DTO.RolloutInfo rollout = GetDataFromXml.GetReconstructedInfo(this.ReconstructionPath);
                //Add a rollout record with the info in the rollout record.
                UpdateXmlFile.AddRolloutRecord(rollout);
            }
        }
    }
}
