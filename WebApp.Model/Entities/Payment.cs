using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Model.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        public string Provider { get; set; } = "MoMo";
        public string ProviderPaymentId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VNĐ";
        public PaymentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? RawResponse { get; set; }
    }

    public enum PaymentStatus
    {
        Pending = 0,
        Success = 1,
        Failed = 2,
        Cancelled = 3
    }
}
