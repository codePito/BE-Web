using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model.Entities;

namespace WebApp.Repository.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart?> GetCartByUserIdAsync(int userId);
        Task AddCartAsync(Cart cart);
        Task AddItemAsync(CartItem item);
        Task UpdateItemAsync(CartItem item);
        Task RemoveItemAsync(CartItem item);
        Task SaveChangesAsync();
    }
}
