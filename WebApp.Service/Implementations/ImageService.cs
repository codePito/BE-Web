using Amazon.Runtime.Internal.Util;
using Amazon.S3;
using Amazon.S3.Model;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model.Response;
using WebApp.Repository.Interfaces;
using WebApp.Service.Interfaces;

namespace WebApp.Service.Implementations
{
    public class ImageService : IImageService
    {
        private readonly IImageRepository _repo;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly ILogger<ImageService> _logger;
        private readonly IAmazonS3 _s3Client;

        private readonly string _bucketName;
        private readonly string _publicUrl;

        private readonly string[] _allowExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly long _maxFileSize = 5 * 1024 * 1024;

        public ImageService(IImageRepository repo, IMapper mapper, IConfiguration config, ILogger<ImageService> logger, IAmazonS3 s3Client)
        {
            _repo = repo;
            _mapper = mapper;
            _config = config;
            _logger = logger;
            _s3Client = s3Client;

            _bucketName = config["CloudflareR2:BucketName"] ?? throw new ArgumentNullException("CloudflareR2:BucketName is not configured");

            _publicUrl = config["CloudflareR2:PublicUrl"] ?? throw new ArgumentNullException("CloudflareR2:PublicUrl is not configured");
        }

        public async Task<bool> DeleteImageAsync(int imageId, int userId)
        {
            var image = await _repo.GetByIdAsync(imageId);
            if (image == null) return false;

            image.IsDeleted = true;
            image.DeletedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(image);

            _logger.LogInformation("Image {ImageId} soft deleted by user {UserId}", imageId, userId);

            return true;
        }

        public async Task<List<ImageResponse>> GetImagesByEntityAsync(string entityType, int entityId)
        {
            var images = await _repo.GetByEntityAsync(entityType, entityId);
            return _mapper.Map<List<ImageResponse>>(images);
        }

        public async Task<ImageResponse?> GetPrimaryImagesAsync(string entityType, int entityId)
        {
            var image = await _repo.GetPrimaryImageAsync(entityType, entityId);
            return image == null ? null : _mapper.Map<ImageResponse>(image);
        }

        public async Task<bool> PermanentDeleteAsync(int imageId)
        {
            try
            {
                var image = await _repo.GetByIdAsync(imageId);
                if (image == null) return false;

                // Delete from R2
                await DeleteFromR2Async(image.StorageKey);

                _logger.LogInformation("Deleted from R2: {StorageKey}", image.StorageKey);

                // Delete from database
                await _repo.DeleteAsync(imageId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error permanently deleting image {ImageId}", imageId);
                return false;
            }
        }

        public async Task<bool> SetPrimaryImageAsync(int imageId, string entityType, int entityId)
        {
            await _repo.UnsetPrimaryAsync(entityType, entityId);

            // Set new primary
            var image = await _repo.GetByIdAsync(imageId);
            if (image == null) return false;

            image.IsPrimary = true;
            await _repo.UpdateAsync(image);

            _logger.LogInformation(
                "Image {ImageId} set as primary for {EntityType} #{EntityId}",
                imageId, entityType, entityId);

            return true;
        }

        public async Task<ImageResponse> UploadImageAsync(IFormFile file, string entityType, int entityId, int uploadedBy, bool isPrimary = false)
        {
            try
            {
                _logger.LogInformation("Uploading image for {EntityType} #{EntityId}", entityType, entityId);
                ValidateFile(file);

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var fileName = $"{Guid.NewGuid()}{extension}";
                var storageKey = $"{entityType.ToLower()}/{entityId}/{fileName}";

                _logger.LogDebug("Storage key: {StorageKey}", storageKey);

                using var optimizedStream = await OptimizeImageAsync(file);

                optimizedStream.Position = 0;
                using var image = await SixLabors.ImageSharp.Image.LoadAsync(optimizedStream);
                var width = image.Width;
                var height = image.Height;

                optimizedStream.Position = 0;
                await UploadToR2Async(storageKey, optimizedStream, file.ContentType);

                var publicUrl = $"{_publicUrl}/{storageKey}";

                _logger.LogInformation("Image upload successful: {Url}", publicUrl);

                if (isPrimary)
                {
                    await _repo.UnsetPrimaryAsync(entityType, entityId);
                }

                var imageEntity = new WebApp.Model.Entities.Image
                {
                    Url = publicUrl,
                    OriginalFileName = file.FileName,
                    StorageKey = storageKey,
                    EntityType = entityType,
                    EntityId = entityId,
                    IsPrimary = isPrimary,
                    FileSize = file.Length,
                    MimeType = file.ContentType,
                    Width = width,
                    Height = height,
                    UploadedBy = uploadedBy,
                    UploadedAt = DateTime.UtcNow,
                    StorageProvider = "CloudflareR2"
                };

                var saved = await _repo.AddAsynn(imageEntity);

                return _mapper.Map<ImageResponse>(saved);
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "R2 error uploading image");
                throw new Exception("Failed to upload image to R2" + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                throw new Exception("Failed to update image: " + ex.Message, ex);
            }
        }

        public async Task<List<ImageResponse>> UploadMultiImagesAsync(IFormFileCollection files, string entityType, int entityId, int uploadedBy)
        {
            var results = new List<ImageResponse>();

            for (int i = 0; i <= files.Count; i++)
            {
                try
                {
                    var isPrimary = i == 0;

                    var result = await UploadImageAsync(
                        files[i],
                        entityType,
                        entityId,
                        uploadedBy,
                        isPrimary
                        );

                    results.Add(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading file {fileName}", files[i].FileName);
                }
            }
            return results;
        }

        private void ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");
            if (file.Length > _maxFileSize)
                throw new ArgumentException($"File size exceeds {_maxFileSize / 1024 / 1024} MB limit");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowExtensions.Contains(extension))
                throw new ArgumentException($"File type {extension} is not allowed");

            if (!file.ContentType.StartsWith("image/"))
                throw new ArgumentException("File is not an image");
        }

        private async Task<Stream> OptimizeImageAsync(IFormFile file)
        {
            try
            {
                using var image = await SixLabors.ImageSharp.Image.LoadAsync(file.OpenReadStream());

                if (image.Width > 1920 || image.Height > 1920)
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(1920, 1920),
                        Mode = ResizeMode.Max
                    }));

                }
                var outputStream = new MemoryStream();

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (extension == ".jpg" || extension == ".jpeg")
                {
                    await image.SaveAsJpegAsync(outputStream,
                        new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder
                        {
                            Quality = 85
                        });
                }
                else if (extension == ".png")
                {
                    await image.SaveAsPngAsync(outputStream,
                        new SixLabors.ImageSharp.Formats.Png.PngEncoder
                        {
                            CompressionLevel = SixLabors.ImageSharp.Formats.Png
                                .PngCompressionLevel.BestCompression
                        });
                }
                else
                {
                    await image.SaveAsync(outputStream, image.Metadata.DecodedImageFormat);
                }

                outputStream.Position = 0;
                return outputStream;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error optimize image, using original");

                var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;
                return stream;
            }
        }

        private async Task UploadToR2Async(
            string key,
            Stream stream,
            string contentType)
        {
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = stream,
                ContentType = contentType,
                CannedACL = S3CannedACL.PublicRead
            };

            await _s3Client.PutObjectAsync(request);
        }

        private async Task DeleteFromR2Async(string key)
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(request);
        }
    }
}
