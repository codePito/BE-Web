using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApp.Model.Entities;
using WebApp.Model.Request;
using WebApp.Service.Interfaces;

namespace WebApp.Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;
        public UserController(IUserService service)
        {
            _service = service;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRequest request)
        {
            await _service.RegisterAsync(request);
            return Ok("Registed Successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            var token = await _service.AuthenticateAysnc(email, password);
            if (token == null) return Unauthorized("Invalid Credential");
            return Ok(new { token });
        }
        [HttpGet]
        public async Task<IActionResult> GetByUsername(string email)
        {
            var user = await _service.GetByUsernameAsync(email);
            return Ok(user);
        }
        
    }
}
