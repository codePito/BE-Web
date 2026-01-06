using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model.Entities;

namespace WebApp.Model.Response
{
    public class OrderResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? ShippingAddress { get; set; }
        public string Currency {  get; set; }
        public int Status { get; set; }
        public List<OrderItemResponse> Items { get; set; } = new();
        public DateTime? PaymentExpiry { get; set; }

    }
}
