using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model.Entities;

namespace WebApp.Repository.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string email);
        Task AddAsync(User user);
        Task CreateCartAsync(Cart cart);
        Task SaveChangesAsync();
    }
}
