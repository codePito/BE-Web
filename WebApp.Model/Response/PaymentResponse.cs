using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Model.Response
{
    public class PaymentResponse
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Provider {  get; set; } = string.Empty;
        public string ProviderPaymentId { get; set; } = string.Empty;
        public decimal amout { get; set; }
        public string Currency { get; set; } = "VND";
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } 
    }
}
