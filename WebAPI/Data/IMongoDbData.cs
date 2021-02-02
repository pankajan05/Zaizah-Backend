using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Model;

namespace WebAPI.Data
{
    public interface IMongoDbData
    {
        Task<UserDetails> GetUserDetails(string userName, string password);
        Task<UserDetails> GetUserDetails(string userName);
        Task saveUserDetails(UserDetails userDetails);
        Task<string> Update(string userName, UserDetails userdetails);
    }
}
