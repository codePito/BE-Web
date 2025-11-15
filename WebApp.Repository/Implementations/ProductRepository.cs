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

        public void Add(Product product) => _context.Products.Add(product);
        

        public void Delete(int id)
        {
            var product = _context.Products.Find(id);
            if(product != null)
            {
                _context.Products.Remove(product);
            }
        }
        

        public Product? GetByID(int id) => _context.Products.Find(id);
        

        public IEnumerable<Product> GetProducts() => _context.Products.ToList();
        

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Update(Product product) => _context.Products.Update(product);
        
    }
}
