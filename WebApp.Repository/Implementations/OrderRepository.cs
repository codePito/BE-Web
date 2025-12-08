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
    public class OrderRepository : IOrderRepository
    {
        private readonly WebContext _context;
        public OrderRepository(WebContext context) => _context = context;
        public async Task<Order> AddAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task DeleteAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return;
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .ToListAsync();
        }

        public Task SaveChangesAsync() => _context.SaveChangesAsync();

        public Task UpdateAsync(Order order)
        {
            throw new NotImplementedException();
        }
    }
}
