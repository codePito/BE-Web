using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using WebApp.Model.Entities;

namespace WebApp.Model.Data
{
    public static class DataSeeder
    {
        private static readonly TimeZoneInfo VietnamTimeZone = GetVietnamTimeZone();

        private static TimeZoneInfo GetVietnamTimeZone()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            else
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
        }

        private static DateTime VietnamNow => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);

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
                    CreatedAt = VietnamNow
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
