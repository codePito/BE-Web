using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Model.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public string? ShippingAddress { get; set; }
        public string Currency {  get; set; } = "VNĐ";
        public DateTime? PaymentExpiry { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }

    public enum OrderStatus
    {
        Pending = 0,
        PaymentPending = 1,
        Paid = 2,
        Cancelled = 3,
        Failed = 4
    }
}
