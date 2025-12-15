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
    public class ImagesRepository : IImageRepository
    {
        private readonly WebContext _context;
        public ImagesRepository(WebContext context)
        {
            _context = context;
        }

        public async Task<Image> AddAsynn(Image image)
        {
            _context.Images.Add(image);
            await _context.SaveChangesAsync();
            return image;
        }

        public async Task DeleteAsync(int id)
        {
            var image = await _context.Images.FindAsync(id);
            if (image != null)
            {
                _context.Images.Remove(image);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Image>> GetByEntityAsync(string entityType, int entityId)
        {
            return await _context.Images.Where(i => i.EntityType == entityType && i.EntityId == entityId && !i.IsDeleted)
                .OrderByDescending(i => i.IsPrimary)
                .ThenBy(i => i.DisplayOrder)
                .ThenBy(i => i.UploadedAt)
                .ToListAsync();
        }

        public async Task<Image?> GetByIdAsync(int id)
        {
            return await _context.Images.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        }

        public async Task<List<Image>> GetByUserAsync(int userId)
        {
            return await _context.Images.Where(i => i.UploadedBy == userId && !i.IsDeleted).ToListAsync();
        }

        public Task<Image?> GetPrimaryImageAsync(string entityType, int entityId)
        {
            return _context.Images.FirstOrDefaultAsync(i => i.EntityType == entityType && i.EntityId == entityId && i.IsPrimary && !i.IsDeleted);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task UnsetPrimaryAsync(string entityType, int entityId)
        {
            var images = await _context.Images.Where(i => i.EntityType == entityType && i.EntityId == entityId && i.IsPrimary).ToListAsync();
            foreach (var img in images)
            {
                img.IsPrimary = false;
            }
            if (images.Any()) await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Image image)
        {
            _context.Images.Update(image);
            await _context.SaveChangesAsync();
        }
    }
}
