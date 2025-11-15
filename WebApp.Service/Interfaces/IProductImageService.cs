using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model.Entities;

namespace WebApp.Service.Interfaces
{
    public interface IProductImageService
    {
        Task<List<ProductImage>> GetImages(int productId);
        Task<ProductImage> SaveImage(int productId, string filePath);
        Task<bool> RemoveImage(int id);
    }
}
