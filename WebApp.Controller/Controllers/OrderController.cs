using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Model.Entities;
using WebApp.Model.Request;
using WebApp.Service.Interfaces;

namespace WebApp.Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _service;
        public OrderController(IOrderService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrderRequest request)
        {
            var userIdClaim = User.FindFirst("id");
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim.Value);
            var response = await _service.CreateOrderAsync(request, userId);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var response = await _service.GetByIdAsync(id);
            if(response == null) return NotFound();
            return Ok(response);
        }

        [HttpGet("/user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetByUser(int userId)
        {
            //var userId = int.Parse(User.FindFirst("id").Value);
            var response = await _service.GetByUserIdAsync(userId);
            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteOrderAsync(id);
            return Ok(ok);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> Update(int id, [FromBody] OrderStatus status)
        {
            var ok = await _service.UpdateOrderStatusAsync(id, status);
            if (!ok) return NotFound();
            return Ok(ok);
        }
    }
}
