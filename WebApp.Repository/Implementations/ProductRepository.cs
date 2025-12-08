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
                                .Include(p => p.Images)
                                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return false;
            _context.Products.Remove(product);
            return true;
        }
        

        public Product? GetByID(int id) => _context.Products
                                                .Include(p => p.Images)
                                                .Include(p => p.Category)
                                                .FirstOrDefault(p => p.Id == id);
        

        public async Task<IEnumerable<Product>> GetProducts() =>  await _context.Products
                                                        .Include(p => p.Images)
                                                        .Include(p => p.Category)
                                                        .ToListAsync();
        

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Update(Product product) => _context.Products.Update(product);
        
    }
}
