using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Service.Interfaces;
using WebApp.Repository.Interfaces;
using WebApp.Model;
using WebApp.Model.Request;
using WebApp.Model.Response;
using AutoMapper;

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

    }
    
}
