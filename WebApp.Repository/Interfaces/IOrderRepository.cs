using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model.Entities;

namespace WebApp.Repository.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> AddAsync(Order order);
        Task<Order?> GetByIdAsync(int id);
        Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Order>> GetAllAsync();
        Task DeleteAsync(int id);
        Task UpdateAsync(Order order);
        Task<IEnumerable<Order>> GetExpiredPendingOrdersAsync(int minutesAgo);
        Task SaveChangesAsync();
    }
}
