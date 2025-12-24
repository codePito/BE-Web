using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using WebApp.Model.Entities;

namespace WebApp.Model.Data
{
    public static class DataSeeder
    {
        public static void SeedData(WebContext context)
        {
            // Seed Admin User
            if (!context.Users.Any(u => u.Email == "admin@gmail.com"))
            {
                var adminUser = new User
                {
                    UserName = "Admin",
                    Email = "admin@gmail.com",
                    PasswordHash = HashPassword("123456"),
                    Role = "Admin",
                    Address = "Admin Address",
                    PhoneNumber = "0000000000",
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(adminUser);
                context.SaveChanges();
            }
        }

        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
