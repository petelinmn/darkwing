using System;
using System.Security.Cryptography;
using System.Text;

namespace DarkWing.Connector.Binance.Http
{
    public static class SignatureHelper 
    {
        /// <summary>Signs the given source with the given key using HMAC SHA256.</summary>
        public static string Sign(string source, string key) {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            using var hmacsha256 = new HMACSHA256(keyBytes);
            var sourceBytes = Encoding.UTF8.GetBytes(source);

            var hash = hmacsha256.ComputeHash(sourceBytes);

            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
