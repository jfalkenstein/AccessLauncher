using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessLauncher.Domain
{
    /* This class is solely for the purpose of getting data out of the User_Login_Info table of the Project Manager
     * Database.
     * 
     * It has two public methods, both static: GetUsersFromAccess and GetUserByUsername.
     * 
     * GetUsersFromAccess pulls a List<DTO.User> from the user_login_info table.
     * 
     * GetUserByUserName uses a lambda expression to produce the single user with the specified username, otherwise it produce a
     * null user.
     */
    
    public class UserDbManager
    {
        private readonly Interfaces.IUserInfoRepository _userRepository;
        public UserDbManager(Interfaces.IUserInfoRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public List<DTO.User> GetUsers()
        {
            return _userRepository.GetUsers();
        }

        public DTO.User GetUserByUsername(string userName)
        {
            DTO.User user = GetUsers().SingleOrDefault(p => p.UserName == userName);
            return user;

        }

        public static String MakeConnectionString(string PathToAccessBackEnd)
        {
            var builder = new System.Data.OleDb.OleDbConnectionStringBuilder();
            builder.Provider = "Microsoft.ACE.OLEDB.12.0";
            builder.DataSource = PathToAccessBackEnd;
            return builder.ConnectionString;
        }
    }
}
