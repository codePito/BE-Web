using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model.Entities;

namespace WebApp.Service.Interfaces
{
    public interface IUserService
    {
        Task<string?> AuthenticateAysnc(string username, string password);
        Task RegisterAsync(User user, string password);
        Task<User> GetByUsernameAsync(string username);
    }
}
