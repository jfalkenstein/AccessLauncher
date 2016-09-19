using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessLauncher.DTO.Enums
{
    /*This Enumeration is used throughout the system to specify which end is being referred to.
     * Usually, this is a matter of perspective, depending on whether the front end launcher or back end
     * tool is currently running the code.
     * 
     */
    
    public enum BackEndOrFrontEndEnum
    {
        BackEnd,
        FrontEnd
    }
}
