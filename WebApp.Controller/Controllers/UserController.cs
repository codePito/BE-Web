using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApp.Model.Entities;
using WebApp.Model.Request;
using WebApp.Service.Interfaces;

namespace WebApp.Controller.Controllers
{
    [Route("api/user")]
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
        [HttpGet("{email}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByUsername(string email)
        {
            var user = await _service.GetByUsernameAsync(email);
            return Ok(user);
        }

        [HttpPut("{id}/profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateProfileRequest request)
        {
            var userIdClaim = User.FindFirst("id");
            if (userIdClaim == null) return Unauthorized();

            int currentUserId = int.Parse(userIdClaim.Value);

            //// Chỉ cho phép user update profile của chính mình (trừ admin)
            //if (currentUserId != id && !User.IsInRole("Admin"))
            //{
            //    return Forbid();
            //}

            await _service.UpdateProfileAsync(id, request);
            return Ok(new { message = "Profile updated successfully" });
        }

        [HttpGet("/api/users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers()
        {
            var result = await _service.GetUsers();
            return Ok(result);
        }
        
    }
}
