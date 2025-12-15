using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model.Response;

namespace WebApp.Service.Interfaces
{
    public interface IImageService
    {
        Task<ImageResponse> UploadImageAsync(IFormFile file, string entityType, int entityId, int uploadedBy, bool isPrimary = false);
        Task<List<ImageResponse>> UploadMultiImagesAsync(IFormFileCollection files, string entityType, int entityId, int uploadedBy);
        Task<List<ImageResponse>> GetImagesByEntityAsync(string entityType, int entityId);
        Task<ImageResponse?> GetPrimaryImagesAsync(string entityType, int entityId);
        Task<bool> DeleteImageAsync(int imageId, int userId);
        Task<bool> SetPrimaryImageAsync(int imageId, string entityType, int entityId);
        Task<bool> PermanentDeleteAsync(int imageId);
    }
}
