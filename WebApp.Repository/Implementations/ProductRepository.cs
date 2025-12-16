using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model;
using WebApp.Model.Data;
using WebApp.Repository.Interfaces;

namespace WebApp.Repository.Implementations
{
    public class ProductRepository : IProductRepository
    {
        private readonly WebContext _context;
        public ProductRepository(WebContext context)
        {
            _context = context;
        }

        public async Task Add(Product product)
        {
            _context.Products.Add(product);
        }
        

        public async Task<bool> Delete(int id)
        {
            var product = await _context.Products
                                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return false;
            _context.Products.Remove(product);
            return true;
        }
        

        public Product? GetByID(int id)
        {
            var product = _context.Products
                                .Include(p => p.Category)
                                .FirstOrDefault(p => p.Id == id);

            if (product != null)
            {
                var images = _context.Images.Where(i => i.EntityType == "Product" && i.EntityId == product.Id && !i.IsDeleted)
                                            .OrderByDescending(i => i.IsPrimary)
                                            .ThenBy(i => i.DisplayOrder)
                                            .ToList();

                product.PrimaryImageUrl = images.FirstOrDefault(i => i.IsPrimary)?.Url;
                product.ImageUrls = images.Select(i => i.Url).ToList();
            }

            return product;
        }
        

        public async Task<IEnumerable<Product>> GetProducts()
        {
            var products = await _context.Products
                                .Include(p => p.Category)
                                .ToListAsync();

            var productIds = products.Select(p =>  p.Id).ToList();
            var images = await _context.Images.Where(i => i.EntityType == "Product" && productIds.Contains(i.EntityId) && !i.IsDeleted)
                                        .OrderByDescending(i => i.IsPrimary)
                                        .ThenBy(i => i.DisplayOrder)
                                        .ToListAsync();

            foreach (var product in products)
            {
                var productImages = images.Where(i => i.EntityId == product.Id).ToList();
                product.PrimaryImageUrl = productImages.FirstOrDefault(i => i.IsPrimary)?.Url;
                product.ImageUrls = productImages.Select(i => i.Url).ToList();
            }

            return products;
        }
        

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Update(Product product) => _context.Products.Update(product);
        
    }
}
