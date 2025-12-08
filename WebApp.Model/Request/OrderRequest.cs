using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Model.Request
{
    public class OrderRequest
    {
        public int UserId { get; set; }
        public List<OrderItemRequest> Items { get; set; } = new();
        public string Currency { get; set; } = "VND";
    }
}
