using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApp.Service.Interfaces;

namespace WebApp.Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductImageController : ControllerBase
    {
        private readonly IProductImageService _service;
        private readonly IWebHostEnvironment _env;
        public ProductImageController(IProductImageService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        [HttpPost("{productId}")]
        public async Task<IActionResult> UploadImage(int productId, IFormFile file)
        {
            if(file == null || file.Length == 0)
                return BadRequest("File rỗng");
            string folder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            string url = $"/uploads/{fileName}";
            var result = await _service.SaveImage(productId, url);

            return Ok(result);
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetImage(int productId)
        {
            var imgs = await _service.GetImages(productId);
            var urls = imgs.Select(x => x.FilePath);

            // Hoặc trả về Id + FilePath
            var result = imgs.Select(x => new { x.Id, x.FilePath });
            return Ok(result);
        }

        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteImage(int id)
        //{
        //    var img = await _service.GetImages(id); // lấy 1 ảnh
        //    if (img == null || string.IsNullOrEmpty(img.FilePath))
        //        return NotFound("Không tìm thấy ảnh.");

        //    // Tạo đường dẫn vật lý
        //    var physicalPath = Path.Combine(_env.WebRootPath ?? string.Empty, img.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        //    // Xóa file nếu tồn tại
        //    if (System.IO.File.Exists(physicalPath))
        //        System.IO.File.Delete(physicalPath);

        //    var deleted = await _service.RemoveImage(id);
        //    if (!deleted) return BadRequest("Không thể xóa ảnh!");

        //    return Ok(new { message = "Đã xóa ảnh" });
        //}
    }
}
