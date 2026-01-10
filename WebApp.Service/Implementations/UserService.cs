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
using BCrypt.Net;

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

        public async Task<string?> AuthenticateAysnc(SignInRequest request)
        {
            var user = await _repo.GetByUsernameAsync(request.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) return null;


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("id", user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim("createdAt", user.CreatedAt.ToString("yyyy-MM-dd")),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
                new Claim(ClaimTypes.StreetAddress, user.Address)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
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
            var checkUser = await _repo.GetByUsernameAsync(request.Email);
            if(checkUser != null)
            {
                throw new Exception("User already exists");
            }
            var entity = _mapper.Map<User>(request);
            entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            
            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();

        }

        public async Task UpdateProfileAsync(int userId, UpdateProfileRequest request)
        {
            var user = await _repo.GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            // Update fields
            user.UserName = request.UserName;
            user.PhoneNumber = request.PhoneNumber;
            user.Address = request.Address;

            _repo.Update(user);
            await _repo.SaveChangesAsync();
        }
    }
}
