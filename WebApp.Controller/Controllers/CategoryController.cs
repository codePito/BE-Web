using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApp.Model.Entities;
using WebApp.Model.Request;
using WebApp.Service.Interfaces;

namespace WebApp.Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;
        public CategoryController(ICategoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = _service.GetCategoriesAsync();
            return Ok(categories);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory([FromBody] CategoryRequest request)
        {
            await _service.AddCategoryAsync(request);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCategory(int id, CategoryRequest request)
        {
            if(id != request.Id)
            {
                return BadRequest();
            }
            await _service.UpdateCategoryAsync(request);
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            await _service.Delete(id);
            return NoContent();
        }
    }
}
