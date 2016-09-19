using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AccessLauncher.FeUninstaller
{
    /* This is a simple program that deletes the front end implementation. It exists because it is difficult
     * to make an application delete itself and its references -- which is what I needed the front end to do.
     * This uninstaller is sent to the rollout directory when the back end is installed. If a user is deauthorized,
     * the uninstaller will be activated.
     */
    
    class Program
    {
        static void Main(string[] args)
        {
            string AppDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PBUSA\\Access Launcher";
            foreach (var file in Directory.GetFiles(AppDirectory))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception)
                {
                }
            }

            foreach(var directory in Directory.GetDirectories(AppDirectory))
            {
                try
                {
                    Directory.Delete(directory, true);
                }
                catch (Exception)
                {
                }
            }
            try
            { 
                Directory.Delete(AppDirectory, true); 
            }
            catch (Exception)
            {
            }
            try
            {
                File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\PB Database.lnk");
            }
            catch (Exception)
            {
            }
        }
    }
}
