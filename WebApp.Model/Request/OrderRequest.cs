using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Model.Request
{
    public class OrderRequest
    {
        [Required(ErrorMessage = "UserId is required")]
        public int UserId { get; set; }
        [Required(ErrorMessage = "At least one order item is required")]
        public List<OrderItemRequest> Items { get; set; } = new();
        public string Currency { get; set; } = "VND";

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Address cannot exceed 200 characters")]
        public string ShippingAddress { get; set; } = string.Empty;
    }
}
