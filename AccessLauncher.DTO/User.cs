using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessLauncher.DTO
{
    /* This is a plain user class to carry user information between layers (primarily between
     * the AccessDBInterface layer and the Front End layer.
     */
    public class User
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public Enums.UserTypeEnum UserType { get; set; }
        public string UserName { get; set; }
    }
}
