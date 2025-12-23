using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApp.Model.Entities;
using WebApp.Service.Interfaces;

namespace WebApp.Controller.Controllers
{
    [Route("api/image")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _service;
        private readonly ILogger<ImageController> _logger;

        public ImageController(IImageService service, ILogger<ImageController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("id");
            if (userIdClaim == null)
                throw new UnauthorizedAccessException("User ID not found.");
            return int.Parse(userIdClaim.Value);
        }

        [HttpPost("product/{productId}")]
        public async Task<IActionResult> UploadProductImage(int productId, IFormFile file, [FromQuery] bool isPrimary = false)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _service.UploadImageAsync(file, ImageEntityTypes.Product, productId, userId, isPrimary);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading product image");
                return StatusCode(500, new { message = "Failed to upload image" });
            }
        }

        [HttpPost("product/{productId}/multiple")]
        public async Task<IActionResult> UploadMultipleProductImages(int productId, [FromForm] IFormFileCollection files)
        {
            try
            {
                if (files == null || files.Count == 0) 
                    return BadRequest(new { message = "No files provided" });

                var userId = GetCurrentUserId();
                var results = await _service.UploadMultiImagesAsync(files, ImageEntityTypes.Product, productId, userId);

                return Ok(new
                {
                  UploadedCount = results.Count,
                  images = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading multiple product images");
                return StatusCode(500, new { message = "Failed to upload images" });
            }
        }

        [HttpPost("user/avatar")]
        public async Task<IActionResult> UploadUserAvatar(IFormFile file)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _service.UploadImageAsync(file, ImageEntityTypes.User, userId, userId, true);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading user avatar");
                return StatusCode(500, new { message = "Failed to upload avatar" });
            }
        }

        [HttpPost("category/{categoryId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadCategoryImage(int categoryId, IFormFile file)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _service.UploadImageAsync(file, ImageEntityTypes.Category, categoryId, userId, false);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading category image");
                return StatusCode(500, new { message = "Failed to upload image" });
            }
        }

        [HttpGet("{entityType}/{entityId}")]
        public async Task<IActionResult> GetEntityImages(string entityType, int entityId)
        {
            try
            {
                var images = await _service.GetImagesByEntityAsync(entityType, entityId);
                return Ok(images);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entity images");
                return StatusCode(500, new { message = "Failed to get images" });
            }
        }

        [HttpGet("{entityType}/{entityId}/primary")]
        public async Task<IActionResult> GetPrimaryImage(string entityType, int entityId)
        {
            try
            {
                var image = await _service.GetPrimaryImagesAsync(entityType, entityId);
                if (image == null) return NotFound(new { message = "Primary image not found" });

                return Ok(image);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get primary image");
                return StatusCode(500, new { message = "Failed to get image" });
            }
        }

        [HttpPut("{imageId}/set-primary")]
        public async Task<IActionResult> SetPrimaryImage(int imageId, [FromQuery] string entityType, [FromQuery] int entityId)
        {
            try
            {
                var success = await _service.SetPrimaryImageAsync(imageId, entityType, entityId);
                if (!success) return NotFound(new { message = "Image not found or could not be set as primary" });

                return Ok(new { message = "Primary image updated" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting primary image");
                return StatusCode(500, new { message = "Failed to update primary image" });
            }
        }

        //soft delete
        [HttpDelete("{imageId}")]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _service.DeleteImageAsync(imageId, userId);
                if(!success) return NotFound(new { message = "Image not found or could not be deleted" });

                return Ok(new { message = "Image deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image");
                return StatusCode(500, new { message = "Failed to delete image" });
            }
        }

        [HttpDelete("{imageId}/permanent")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PermanentDeleteImage(int imageId)
        {
            try
            {
                var success = await _service.PermanentDeleteAsync(imageId);

                if (!success) return NotFound(new { message = "Image not found or could not be permanently deleted" });
                return Ok(new { message = "Image permanently deleted" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error permanently deleting image");
                return StatusCode(500, new { message = "Failed to permanently delete image" });
            }
        }
    }
}
