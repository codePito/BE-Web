using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model.Entities;

namespace WebApp.Repository.Interfaces
{
    public interface IProductImageRepository
    {
        Task<List<ProductImage>> GetImagesByProduct(int productId);
        Task<ProductImage> AddImage(ProductImage image);
        Task<bool> DeleteImage(int id);
    }
}
