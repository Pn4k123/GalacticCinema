using System.Security.Cryptography;
using System.Text;

namespace BookingMovieTicket.Helper
{
    public static class ZaloPayHelper
    {
        public static string HmacSHA256(string inputData, string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA256(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                
                return BitConverter.ToString(hashValue).Replace("-", "").ToLower();
            }
        }
    }
}