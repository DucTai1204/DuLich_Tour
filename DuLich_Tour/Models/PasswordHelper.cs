using System;
using System.Security.Cryptography;

namespace DuLich_Tour.Models
{
    public static class PasswordHelper
    {
        // Format: {iterations}.{saltBase64}.{hashBase64}
        public static string HashPassword(string password, int iterations = 10000)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            using (var rng = new RNGCryptoServiceProvider())
            {
                var salt = new byte[16];
                rng.GetBytes(salt);
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
                {
                    var hash = pbkdf2.GetBytes(32);
                    return $"{iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
                }
            }
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrWhiteSpace(storedHash)) return false;

            var parts = storedHash.Split('.');
            if (parts.Length != 3) return false;

            int iterations = int.Parse(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var hash = Convert.FromBase64String(parts[2]);

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                var computed = pbkdf2.GetBytes(hash.Length);
                for (int i = 0; i < hash.Length; i++)
                    if (computed[i] != hash[i]) return false;
                return true;
            }
        }
    }
}