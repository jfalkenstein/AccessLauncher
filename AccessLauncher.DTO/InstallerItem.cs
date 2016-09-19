using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AccessLauncher.DTO
{
    /* This class is responsible for carrying the basic information necessary for installations between layers.
     * An installerItem has a simple SourceFilePath and a DestinationFilePath. Whenever the ExecuteCopy method is
     * called, it copies the source to the destination, overwriting any existing file located there.
     *
     */
   public class InstallerItem
    {
        public string SourceFilePath { get; set; }
        public string DestinationFilePath { get; set; }
        
        public void ExecuteCopy()
        {
            File.Copy(this.SourceFilePath, this.DestinationFilePath, true);
        }

    }
}
