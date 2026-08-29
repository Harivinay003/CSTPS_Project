using System;
using System.Security.Cryptography;
using System.Text;

namespace ManagementConsole
{
    public static class PasswordHelper
    {
        public static void CreateHash(string password, out string hash, out string salt)
        {
            // generate salt
            var saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            salt = Convert.ToBase64String(saltBytes);

            // derive hash
            using var deriveBytes = new Rfc2898DeriveBytes(password, saltBytes, 100_000, HashAlgorithmName.SHA256);
            var hashBytes = deriveBytes.GetBytes(32);
            hash = Convert.ToBase64String(hashBytes);
        }

        public static bool Verify(string password, string hash, string salt)
        {
            var saltBytes = Convert.FromBase64String(salt);
            using var deriveBytes = new Rfc2898DeriveBytes(password, saltBytes, 100_000, HashAlgorithmName.SHA256);
            var testHash = deriveBytes.GetBytes(32);
            var hashBytes = Convert.FromBase64String(hash);
            return CryptographicOperations.FixedTimeEquals(testHash, hashBytes);
        }
    }
}
