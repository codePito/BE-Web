using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Model.Response
{
    public class ProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        //public string CategoryName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public int SoldCount { get; set; }
        public int StockQuantity { get; set; } 
        public int LowStockThreshold { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsOutOfStock { get; set; }
        public bool IsLowStock { get; set; }
    }
}
