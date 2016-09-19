using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessLauncher.DTO.Enums
{
    /* This is the enumeration for user type. As far as the domain code, we could have
     * any number of user types. If we added a user type, we'd have to modify the 
     * AccessDBInterface layer and the back end tool to reflect it, but the domain code does
     * not rely on a set number. This enumeration is what it is keyed in on for both the names
     * of the user types and the number of user types.
     */
    
    public enum UserTypeEnum
    {
        Admin,
        Associate
    }
}
