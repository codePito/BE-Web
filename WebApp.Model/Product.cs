using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model.Entities;

namespace WebApp.Model
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
        public Category? Category { get; set; }

        [NotMapped]
        public string? PrimaryImageUrl { get; set; }
        [NotMapped]
        public List<string> ImageUrls { get; set; } = new();
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
