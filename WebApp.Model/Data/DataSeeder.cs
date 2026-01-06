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
            if (!context.Users.Any(u => u.Email == "anhtuanadmin@gmail.com"))
            {
                var adminUser = new User
                {
                    UserName = "Admin Anh Tuan",
                    Email = "anhtuanadmin@gmail.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Anhtuan@100103"),
                    Role = "Admin",
                    Address = "Admin Address",
                    PhoneNumber = "0000000000",
                    CreatedAt = DateTime.Now
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
