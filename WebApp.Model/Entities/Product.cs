using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Model.Entities
{
    [Table("Product")]
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public decimal Price { get; set; }
        [Required]
        public string Description { get; set; } = string.Empty;
        public int CategoryID { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        public int StockQuantity { get; set; } = 0;
        public int LowStockThreshold { get; set; } = 5;
        public bool IsAvailable { get; set; } = true;
        public Category? Category { get; set; }

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

        [NotMapped]
        public string? PrimaryImageUrl { get; set; }
        [NotMapped]
        public List<string> ImageUrls { get; set; } = new();

        [NotMapped]
        public bool IsLowStock => StockQuantity <= LowStockThreshold;
        [NotMapped]
        public bool IsOutOfStock => StockQuantity <= 0;
    }
}
