using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AccessLauncher.DTO.Exceptions
{
    public class FileCopyException : System.Exception
    {
        public FileInfo FileThatCouldNotCopy { get; set; }
    }
}
