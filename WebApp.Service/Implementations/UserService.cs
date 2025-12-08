using Microsoft.Extensions.Configuration;
using WebApp.Model.Entities;
using WebApp.Repository.Interfaces;
using WebApp.Service.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using WebApp.Model.Request;
using AutoMapper;
using WebApp.Model.Response;

namespace WebApp.Service.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        public UserService(IUserRepository repo, IConfiguration config, IMapper mapper)
        {
            _repo = repo;
            _config = config;
            _mapper = mapper;
        }

        public async Task<string?> AuthenticateAysnc(string email, string password)
        {
            var user = await _repo.GetByUsernameAsync(email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("id", user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<User> GetByUsernameAsync(string email)
        {
            if (email == null) return null;
            return await _repo.GetByUsernameAsync(email);
        }

        public async Task<IEnumerable<UserResponse>> GetUsers()
        {
            var result = await _repo.GetUsers();
            return _mapper.Map<IEnumerable<UserResponse>>(result);
        }

        public async Task RegisterAsync(UserRequest request)
        {
            var entity = _mapper.Map<User>(request);
            entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();

        }
    }
}
