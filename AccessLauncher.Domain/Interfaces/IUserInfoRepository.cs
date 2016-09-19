using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AccessLauncher.Domain.Interfaces
{
    public interface IUserInfoRepository
    {
        List<DTO.User> GetUsers();
    }
}
