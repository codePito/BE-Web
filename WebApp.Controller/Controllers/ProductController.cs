using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApp.Model;
using WebApp.Model.Request;
using WebApp.Service.Interfaces;

namespace WebApp.Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _service;
        public ProductController(IProductService service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _service.GetProductsAsync();
            return Ok(products);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByID(int id)
        {
            var product = await _service.GetByIDAsync(id);
            if(product == null) return NotFound();
            return Ok(product);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductRequest request)
        {
            var userIdClaim = User.FindFirst("id");
            if(userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim.Value);
            var result = await _service.AddAsync(request, userId);
            return Ok(result);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductRequest request)
        {
            if(id != request.Id) return BadRequest();
            await _service.UpdateAsync(request);
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
