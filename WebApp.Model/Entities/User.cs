using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Model.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string UserName { get; set; } = string.Empty;
        [Required, MaxLength(100)]
        public string Address { get; set; }
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        public string Role { get; set; } = "User"; 
        public Cart? Cart { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
