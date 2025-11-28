using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model.Data;
using WebApp.Model.Entities;
using WebApp.Repository.Interfaces;

namespace WebApp.Repository.Implementations
{
    public class CartRepository : ICartRepository
    {
        private readonly WebContext _context;
        public CartRepository(WebContext context)
        {
            _context = context;
        }
        public async Task AddCartAsync(Cart cart)
        {
            await _context.Carts.AddAsync(cart);
        }

        public async Task AddItemAsync(CartItem item)
        {
            await _context.CartItems.AddAsync(item);
        }

        public async Task<Cart?> GetCartByUserIdAsync(int userId)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public Task RemoveItemAsync(CartItem item)
        {
            _context.CartItems.Remove(item);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public Task UpdateItemAsync(CartItem item)
        {
            _context.CartItems.Update(item);
            return Task.CompletedTask;
        }
    }
}
