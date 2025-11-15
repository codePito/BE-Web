using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Model.Entities
{
    public class ProductImage
    {
        [Key]
        public int Id { get; set; }
       
        public string FileName { get; set; } = string.Empty;
       
        public string FilePath { get; set; } = string.Empty;

        //FK - Product
        public int ProductId { get; set; }
        //Navigation
        public Product? Product { get; set; }
    }
}
