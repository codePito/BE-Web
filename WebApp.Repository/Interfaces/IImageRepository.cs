using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model.Entities;

namespace WebApp.Repository.Interfaces
{
    public interface IImageRepository
    {
        Task<Image> AddAsynn(Image image);
        Task<Image?> GetByIdAsync(int id);
        Task<List<Image>> GetByEntityAsync(string entityType, int entityId);
        Task<Image?> GetPrimaryImageAsync(string entityType, int entityId);
        Task UpdateAsync(Image image);
        Task DeleteAsync(int id);
        Task UnsetPrimaryAsync(string entityType, int entityId);
        Task<List<Image>> GetByUserAsync(int userId);
        Task SaveChangesAsync();
    }
}
