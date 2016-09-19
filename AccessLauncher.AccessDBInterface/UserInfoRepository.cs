using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccessLauncher.Domain.XmlAccess;

namespace AccessLauncher.AccessDBInterface
{
    public class UserInfoRepository : Domain.Interfaces.IUserInfoRepository
    {
        private readonly string _ConnectionString;
        private AccessBEDBTableAdapters.User_Login_InfoTableAdapter _tableAdapter;
        private AccessBEDB.User_Login_InfoDataTable _userInfoTable;
        public UserInfoRepository(DTO.Enums.BackEndOrFrontEndEnum whichEnd)
        {
            _ConnectionString = GetDataFromXml.GetCurrentConnectionString(whichEnd);
            _tableAdapter = new AccessBEDBTableAdapters.User_Login_InfoTableAdapter();
            _tableAdapter.Connection.ConnectionString = _ConnectionString;
            _userInfoTable = new AccessBEDB.User_Login_InfoDataTable();
        }
        public List<DTO.User> GetUsers()
        {
            List<DTO.User> list = new List<DTO.User>();
            
            try
            {
                _tableAdapter.Fill(_userInfoTable);
                foreach (AccessBEDB.User_Login_InfoRow row in _userInfoTable.Rows)
                {
                    if (row.IsNull("Full_Name")) continue;
                    DTO.Enums.UserTypeEnum userType;
                    if (row.Administrator == true)
                    {
                        userType = DTO.Enums.UserTypeEnum.Admin;
                    }
                    else
                    {
                        userType = DTO.Enums.UserTypeEnum.Associate;
                    }
                    var newUser = new DTO.User()
                    {
                        Email = row.User_Email,
                        Name = row.Full_Name, 
                        UserName = row.UserName, 
                        UserType=userType
                    };
                    list.Add(newUser);
                }
            }
            catch (Exception)
            {
                throw new DTO.Exceptions.CouldNotAccessDBException();
            }
            
            return list;
        }
    }
}
