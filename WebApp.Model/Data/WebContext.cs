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
        //public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Image> Images { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Product - Category
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryID)
                .OnDelete(DeleteBehavior.Cascade);

            // Product -> ProductImage (1-n)
            //modelBuilder.Entity<ProductImage>()
            //    .HasOne(pi => pi.Product)
            //    .WithMany(p => p.Images) // navigation property Product.Images
            //    .HasForeignKey(pi => pi.ProductId)
            //    .OnDelete(DeleteBehavior.Cascade);

            //Images
            modelBuilder.Entity<Image>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.EntityType, e.EntityId })
                    .HasDatabaseName("IX_Image_EntityType_EntityId");

                entity.HasIndex(e => e.StorageKey)
               .IsUnique()
               .HasDatabaseName("IX_Image_StorageKey");

                entity.HasIndex(e => new { e.EntityType, e.EntityId, e.IsPrimary })
                    .HasDatabaseName("IX_Image_Primary");

                entity.HasIndex(e => e.UploadedBy)
                    .HasDatabaseName("IX_Image_UploadedBy");

                // Query filter - không query ảnh đã xóa
                entity.HasQueryFilter(i => !i.IsDeleted);

            });
                

            // User - Cart (1-1)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Cart)
                .WithOne(c => c.User)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cart - CartItem (1-n)
            modelBuilder.Entity<Cart>()
                .HasMany(c => c.Items)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(c => c.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            //CartItem - Product (n-1)
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany(p => p.Items)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            //User - Order (1-n)
            modelBuilder.Entity<User>()
                .HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            //Order - OrderItem (1 - n)
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            //Order - Payment (1 - n)
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Payments)
                .WithOne(p => p.Order)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
