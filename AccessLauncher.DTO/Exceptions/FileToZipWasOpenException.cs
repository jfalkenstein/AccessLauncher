using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessLauncher.DTO.Exceptions
{
    public class FileToZipWasOpenException : System.IO.IOException
    {
        public String FileThatCouldNotOpen { get; set; }
        public override string Message
        {
            get
            {
                return _message;
            }
        }
        private string _message;

        public FileToZipWasOpenException(string fileThatCouldNotOpen)
        {
            this.FileThatCouldNotOpen = fileThatCouldNotOpen;
            this._message = "Please close the following file so it can be archived: " + this.FileThatCouldNotOpen;
        }
    }
}
