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

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders.Include(o => o.Items)
                                        .Include(o => o.Payments)
                                        .Include(o => o.User)
                                        .OrderByDescending(o => o.CreatedAt)
                                        .ToListAsync();
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
        public async Task<IEnumerable<Order>> GetExpiredPendingOrdersAsync(int minutesAgo)
        {
            var cutoffTime = DateTime.UtcNow.AddMinutes(-minutesAgo);

            return await _context.Orders
                .Where(o => o.Status == OrderStatus.PaymentPending
                    && (o.PaymentExpiry.HasValue && o.PaymentExpiry.Value < DateTime.UtcNow
                        || o.CreatedAt < cutoffTime)) 
                .Include(o => o.Items)
                .ToListAsync();
        }
        public Task SaveChangesAsync() => _context.SaveChangesAsync();

        public Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            return _context.SaveChangesAsync();
        }
    }
}
