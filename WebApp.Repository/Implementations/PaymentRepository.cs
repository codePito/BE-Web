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
    public class PaymentRepository : IPaymentRepository
    {
        private readonly WebContext _context;
        public PaymentRepository(WebContext context) => _context = context;
        public async Task<Payment> AddAsync(Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<IEnumerable<Payment>> GetByOrderIdAllAsync(int orderId)
        {
            return await _context.Payments.Where(p => p.OrderId == orderId).OrderByDescending(p => p.CreatedAt).ToListAsync();
        }

        public async Task<Payment?> GetByOrderIdAsync(int orderId)
        {
            return await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
        }

        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _context.Payments.FirstOrDefaultAsync(p => p.Id == id);
        }

        public Task SaveChangesAsync() => _context.SaveChangesAsync();

        public async Task UpdateAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
        }
    }
}
