using System.Security.Cryptography;
using System.Text;

namespace WebApp.Helper
{
    public static class Security
    {
        public static string ComputeHmacSha256(string message, string secretKey)
        {
            var encoding = new UTF8Encoding();
            byte[] keyByte = encoding.GetBytes(secretKey);
            byte[] messageBytes = encoding.GetBytes(message);

            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                // Chuyển sang chuỗi Hex và bắt buộc phải lowercase
                string hash = BitConverter.ToString(hashmessage).Replace("-", "").ToLower();
                return hash;
            }
        }
    }
}