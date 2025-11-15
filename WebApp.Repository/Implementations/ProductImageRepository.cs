using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model.Data;
using WebApp.Model.Entities;
using WebApp.Repository.Interfaces;

namespace WebApp.Repository.Implementations
{
    public class ProductImageRepository : IProductImageRepository
    {
        private readonly WebContext _context;
        
        public ProductImageRepository(WebContext context)
        {
            _context = context;
        }
        public async Task<ProductImage> AddImage(ProductImage image)
        {
            _context.ProductImages.Add(image);
            await _context.SaveChangesAsync();
            return image;
        }

        public async Task<bool> DeleteImage(int id)
        {
            var img = _context.ProductImages.Find(id);
            if (img != null)  _context.ProductImages.Remove(img);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ProductImage>> GetImagesByProduct(int productId)
        {
            return await _context.ProductImages.Where(x => x.ProductId == productId).ToListAsync();
        }
    }
}
