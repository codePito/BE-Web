using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Helper
{
    public static class Security
    {
        public static string ComputeHmacSha256(string message, string secretKey)
        {
            var encoding = new System.Text.UTF8Encoding();
            byte[] keyByte = encoding.GetBytes(secretKey);
            byte[] messageBytes = encoding.GetBytes(message);

            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                // Chuyển sang chuỗi Hex và bắt buộc phải .ToLower()
                string hash = BitConverter.ToString(hashmessage).Replace("-", "").ToLower();
                return hash;
            }
        }
    }
}
