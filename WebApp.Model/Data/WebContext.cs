using Microsoft.EntityFrameworkCore;
using WebApp.Model.Entities;

namespace WebApp.Model.Data
{
    public class WebContext : DbContext
    {
        public WebContext(DbContextOptions<WebContext> options) : base(options) { }
        #region
        public DbSet<Product>? Products { get; set; }
        public DbSet<User>? Users { get; set; }
        public DbSet<Category>? Categories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        #endregion
    }
}
