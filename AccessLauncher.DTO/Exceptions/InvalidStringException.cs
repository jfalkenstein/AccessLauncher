using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessLauncher.DTO.Exceptions
{
    public class InvalidStringException : System.Exception
    {
        public string FieldThatWasInValid { get; set; }
    }
}
