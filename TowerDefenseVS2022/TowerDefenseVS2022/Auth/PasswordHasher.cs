using System;
using System.Security.Cryptography;

namespace TowerDefenseVS2022.Auth
{
    public static class PasswordHasher
    {
        public static (string saltB64, string hashB64, int iterations) HashPassword(string password, int iterations = 100_000)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            byte[] hash = PBKDF2(password, salt, iterations, 32);
            return (Convert.ToBase64String(salt), Convert.ToBase64String(hash), iterations);
        }

        public static bool Verify(string password, string saltB64, string hashB64, int iterations)
        {
            byte[] salt = Convert.FromBase64String(saltB64);
            byte[] expected = Convert.FromBase64String(hashB64);
            byte[] actual = PBKDF2(password, salt, iterations, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }

        private static byte[] PBKDF2(string password, byte[] salt, int iterations, int bytes)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(bytes);
        }
    }
}
