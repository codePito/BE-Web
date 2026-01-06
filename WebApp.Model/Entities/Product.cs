using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
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
        public string? Variants { get; set; }
        public DateTime CreatedAt { get; set; }
        public int SoldCount { get; set; } = 0;
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
        [NotMapped]
        public ProductVariants? VariantsData
        {
            get
            {
                if (string.IsNullOrEmpty(Variants)) return null;
                try
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<ProductVariants>(Variants, options);
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                Variants = value == null ? null : JsonSerializer.Serialize(value);
            }
        }
        [NotMapped]
        public int TotalStock
        {
            get
            {
                var variants = VariantsData;
                if (variants?.HasVariants == true && variants.Options?.Any() == true)
                {
                    return variants.Options.Sum(v => v.Stock);
                }
                return StockQuantity;
            }
        }
        public class ProductVariants
        {
            public bool HasVariants { get; set; }
            public List<ProductVariantOption> Options { get; set; } = new();
        }

        public class ProductVariantOption
        {
            public string Id { get; set; } = string.Empty;
            public string? Color { get; set; }
            public string? Size { get; set; }
            public int Stock { get; set; }
            public decimal PriceAdjustment { get; set; } = 0;
            public string? Sku { get; set; }
        }
    }
}
