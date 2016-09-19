using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessLauncher.DTO.AbstractClasses
{
    public abstract class XmlSaveException : Exception
    {
        public string FilePathThatCouldNotSave { get; set; }
    }
}
