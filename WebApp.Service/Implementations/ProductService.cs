using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Service.Interfaces;
using WebApp.Repository.Interfaces;
using WebApp.Model.Request;
using WebApp.Model.Response;
using AutoMapper;
using WebApp.Model.Entities;

namespace WebApp.Service.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        private readonly IMapper _mapper;
        public ProductService(IProductRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductResponse>> GetProductsAsync()
        {
            var products = await _repo.GetProducts();
            return _mapper.Map<IEnumerable<ProductResponse>>(products);
        }
        public async Task<ProductResponse> GetByIDAsync(int id)
        {
            var entity =  _repo.GetByID(id);

            if (entity == null) return null;

            return _mapper.Map<ProductResponse>(entity);
        }
        public async Task<ProductResponse> AddAsync(ProductRequest request, int userId)
        {
            var entity = _mapper.Map<Product>(request);
            entity.CreatedBy = userId;
            await _repo.Add(entity);

            await _repo.SaveChangesAsync();

            return _mapper.Map<ProductResponse>(entity);
        }
        public async Task<ProductResponse> UpdateAsync(ProductRequest request)
        {
            var entity = _mapper.Map<Product>(request);
            _repo.Update(entity);
            await _repo.SaveChangesAsync();
            return _mapper.Map<ProductResponse>(entity);
        }
        public async Task DeleteAsync(int id)
        {
            await _repo.Delete(id);
            await _repo.SaveChangesAsync();
        }

        public async Task UpdateStockAsync(int productId, int quantity)
        {
            var product = _repo.GetByID(productId);
            if (product == null) throw new Exception("Product not found");

            if (quantity < 0) throw new ArgumentException("Stock quantity cannot be negative");

            product.StockQuantity = quantity;

            if (quantity > 0)
            {
                product.IsAvailable = true;
            }
            else
            {
                product.IsAvailable = false;
            }
            _repo.Update(product);
            await _repo.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProductResponse>> GetLowStockProductsAsync()
        {
            var products = await _repo.GetProducts();
            var lowStockProducts = products.Where(p => p.IsLowStock).OrderBy(p => p.StockQuantity);

            return _mapper.Map<IEnumerable<ProductResponse>>(lowStockProducts);
        }

        public async Task<IEnumerable<ProductResponse>> GetOutOfStockProductsAsync()
        {
            var products = await _repo.GetProducts();
            var outOfStockProducts = products.Where(p => p.IsOutOfStock);

            return _mapper.Map<IEnumerable<ProductResponse>>(outOfStockProducts);
        }
    }
    
}
