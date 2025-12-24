using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model.Entities;
using WebApp.Model.Request;
using WebApp.Model.Response;

namespace WebApp.Service.Interfaces
{
    public interface IUserService
    {
        Task<string?> AuthenticateAysnc(string email, string password);
        Task RegisterAsync(UserRequest request);
        Task<IEnumerable<UserResponse>> GetUsers();
        Task<User> GetByUsernameAsync(string email);
        Task UpdateProfileAsync(int userId, UpdateProfileRequest request);
    }
}
