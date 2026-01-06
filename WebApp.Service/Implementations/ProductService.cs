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
        public async Task ReduceStockAsync(int productId, int quantity, string? variantId = null)
        {
            var product = _repo.GetByID(productId);
            if (product == null) throw new Exception("Product not found");

            var variants = product.VariantsData;

            if (variants?.HasVariants == true && !string.IsNullOrEmpty(variantId))
            {
                // Reduce variant stock
                var variant = variants.Options.FirstOrDefault(v => v.Id == variantId);
                if (variant == null) throw new Exception($"Variant {variantId} not found");

                if (variant.Stock < quantity)
                    throw new Exception($"Not enough stock for variant {variantId}, only {variant.Stock} available");

                variant.Stock -= quantity;
                product.VariantsData = variants; // Trigger setter to serialize

                // Update total stock
                product.StockQuantity = product.TotalStock;
            }
            else
            {
                // Reduce product stock (no variants)
                if (product.StockQuantity < quantity)
                    throw new Exception($"Not enough stock, only {product.StockQuantity} available");

                product.StockQuantity -= quantity;
            }

            product.IsAvailable = product.StockQuantity > 0;
            _repo.Update(product);
            await _repo.SaveChangesAsync();
        }

        public async Task RestoreStockAsync(int productId, int quantity, string? variantId = null)
        {
            var product = _repo.GetByID(productId);
            if (product == null) throw new Exception("Product not found");

            var variants = product.VariantsData;

            if (variants?.HasVariants == true && !string.IsNullOrEmpty(variantId))
            {
                // Restore variant stock
                var variant = variants.Options.FirstOrDefault(v => v.Id == variantId);
                if (variant == null) throw new Exception($"Variant {variantId} not found");

                variant.Stock += quantity;
                product.VariantsData = variants; // Trigger setter to serialize

                // Update total stock
                product.StockQuantity = product.TotalStock;
            }
            else
            {
                // Restore product stock (no variants)
                product.StockQuantity += quantity;
            }

            product.IsAvailable = true;
            _repo.Update(product);
            await _repo.SaveChangesAsync();
        }
    }
    
}
