using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model.Entities;
using WebApp.Repository.Implementations;
using WebApp.Repository.Interfaces;
using WebApp.Service.Interfaces;

namespace WebApp.Service.Implementations
{
    public class ProductImageService : IProductImageService
    {
        private readonly IProductImageRepository _repo;
        public ProductImageService(IProductImageRepository repo)
        {
            _repo = repo;
        }

        public Task<List<ProductImage>> GetImages(int productId) => _repo.GetImagesByProduct(productId);

        public Task<bool> RemoveImage(int id) => _repo.DeleteImage(id);

        public Task<ProductImage> SaveImage(int productId, string filePath)
        {
            var img = new ProductImage
            {
                ProductId = productId,
                FilePath = filePath
            };
            return _repo.AddImage(img);
        }
    }
}
