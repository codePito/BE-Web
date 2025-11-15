using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Service.Interfaces;
using WebApp.Repository.Interfaces;
using WebApp.Model;

namespace WebApp.Service.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        public ProductService(IProductRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            return _repo.GetProducts();
        }
        public async Task<Product?> GetByIDAsync(int id)
        {
            return _repo.GetByID(id);
        }
        public async Task AddAsync(Product product)
        {
            _repo.Add(product);
            await _repo.SaveChangesAsync();
        }
        public async Task UpdateAsync(Product product)
        {
            _repo.Update(product);
            await _repo.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            _repo.Delete(id);
            await _repo.SaveChangesAsync();
        }
    }
    
}
