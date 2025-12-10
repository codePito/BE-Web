using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApp.Service.Interfaces;

namespace WebApp.Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _service;
        public CartController(ICartService service)
        {
            _service = service;
        }

        private int UserId => int.Parse(User.FindFirst("id").Value);

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            return Ok(await _service.GetUserCartAsync(UserId));
        }

        [HttpPost]
        public async Task<IActionResult> AddItem(int productId, int quantity = 1)
        {
            return Ok(await _service.AddToCartAsync(UserId, productId, quantity));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateItem(int quantity, int itemId)
        {
            return Ok(await _service.UpdateQuantityAsync(UserId, quantity, itemId));
        }

        [HttpDelete("{itemId}")]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            await _service.RemoveItemAsync(UserId, itemId);
            return NoContent();
        }
    }
}
