using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApp.Model.Entities;
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
        public async Task<IActionResult> Register(User user, string password)
        {
            await _service.RegisterAsync(user, password);
            return Ok("Registed Successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var token = await _service.AuthenticateAysnc(username, password);
            if (token == null) return Unauthorized("Invalid Credential");
            return Ok(new { token });
        }
        [HttpGet]
        public async Task<IActionResult> GetByUsername(string username)
        {
            var user = await _service.GetByUsernameAsync(username);
            return Ok(user);
        }
        
    }
}
