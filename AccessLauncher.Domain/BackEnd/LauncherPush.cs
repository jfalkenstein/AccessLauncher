using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Ionic.Zip;
using AccessLauncher.Domain.XmlAccess;

namespace AccessLauncher.Domain.BackEnd
{
    /* This class handles the process of pushing out new components to users for their front end launcher.
     * Besides constructors, there is only one public method in this class, Execute, which runs the execution of
     * The push action.
     * 
     * To instantiate LauncherPush, you need to pass in a list of FileInfo objects (the files to be pushed out).
     * 
     * For the Execute process, see the comments below on it.
     * 
     */
    
    public class LauncherPush
    {
        public List<FileInfo> FilesToPushOut { get; set; }
        public List<FileInfo> CopiedFiles { get; set; }
        public List<FileInfo> FilesToArchive { get; set; }
        string _installerFilesDirectory;
        int _newLauncherVersion;

        public LauncherPush(List<FileInfo> filesToPushOut)
        {
            this.FilesToPushOut = filesToPushOut;
        }

        public void Execute()
        {
            //1. Get Installer directory;
            this._installerFilesDirectory = PathManager.GetInstallerFilesDirectory(DTO.Enums.BackEndOrFrontEndEnum.BackEnd);
            //2. Get current launcher version
            this._newLauncherVersion = GetDataFromXml.GetCurrentLauncherVersion(this._installerFilesDirectory) + 1;
            
            //3. Archive existing files
            archiveExistingFiles();
            //4. Copy each file in the filelist to the installer directory
            copyFiles();
            //5. Compare the file list with the current manifest
            compareWithManifest();

            //6. Update launcher version in backend.xml
            UpdateXmlFile.SetLauncherVersion(this._newLauncherVersion, this._installerFilesDirectory);
        }
        //This will archive any files that are about to be overwritten, for backup purposes.
        private void archiveExistingFiles()
        {
            this.FilesToArchive = new List<FileInfo>();
            foreach(FileInfo file in FilesToPushOut)
            {
                if(File.Exists(this._installerFilesDirectory + file.Name))
                {
                    this.FilesToArchive.Add(new FileInfo(this._installerFilesDirectory + file.Name));
                }
            }
            if(this.FilesToArchive.Count>0) zipFilesIntoArchive();
        }

        //This facilitiates the archiveExistingFiles method, controlling the process of zipping up the
        //List<FileInfo> FilesToArchive
        private void zipFilesIntoArchive()
        {
            using (ZipFile zip = new ZipFile())
            {
                foreach(FileInfo file in this.FilesToArchive)
                {
                    zip.AddFile(file.FullName, "");
                }
                zip.Save(this._installerFilesDirectory + "Launcher Archive\\" 
                    + "v" + (this._newLauncherVersion - 1).ToString() 
                    + "-" 
                    + DateTime.Now.ToShortDateString().Replace("/", "-")
                    + ".zip");
                zip.Dispose();
            }
        }
        //This method copies all the files in the FilesToPushout list to the installerDirectory,
        //Adding to the list of CopiedFiles for each one.
        private void copyFiles()
        {
            this.CopiedFiles = new List<FileInfo>();
            foreach (FileInfo file in this.FilesToPushOut)
            {
                try
                {
                    file.CopyTo(this._installerFilesDirectory + file.Name, true);
                }
                catch (System.IO.IOException)
                {
                    throw new DTO.Exceptions.FileCopyException() { FileThatCouldNotCopy = file };
                }
                this.CopiedFiles.Add(new FileInfo(this._installerFilesDirectory + file.Name));
            }
        }
        //This will compare the copied files with the current installer manifest files. If a file to be
        //added doesn't exist within the manifest, it will add it to the manifest.
        private void compareWithManifest()
        {
            var manifest = GetDataFromXml.GetInstallerManifest(this._installerFilesDirectory, null);
            List<FileInfo> manifestFiles = manifest.Select(p => new FileInfo(p.SourceFilePath)).ToList();
            foreach(FileInfo file in this.CopiedFiles)
            {
                if(!manifestFiles.Any(p=> p.Name == file.Name))
                {
                    UpdateXmlFile.AddFileToManifest(this._installerFilesDirectory, file);
                }
            }
        }


    }
}
