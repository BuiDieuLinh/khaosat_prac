using System;
using System.Security.Cryptography;

namespace khaosat_api.Services
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16; 
        private const int KeySize = 32;  
        private const int Iterations = 100000;
        private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

        public static string Hash(string password)
        {
            using var algorithm = new Rfc2898DeriveBytes(
                password,
                SaltSize,
                Iterations,
                HashAlgorithm);

            byte[] salt = algorithm.Salt;
            byte[] key = algorithm.GetBytes(KeySize);

            byte[] hashBytes = new byte[SaltSize + KeySize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(key, 0, hashBytes, SaltSize, KeySize);

            return Convert.ToBase64String(hashBytes);
        }

        public static bool Verify(string password, string passwordHash)
        {
            try
            {
                byte[] hashBytes = Convert.FromBase64String(passwordHash);
                if (hashBytes.Length != SaltSize + KeySize)
                    return false;
                // Extract salt
                byte[] salt = new byte[SaltSize];
                Array.Copy(hashBytes, 0, salt, 0, SaltSize);

                // Extract key
                byte[] key = new byte[KeySize];
                Array.Copy(hashBytes, SaltSize, key, 0, KeySize);

                // Hash input password with extracted salt
                using var algorithm = new Rfc2898DeriveBytes(
                    password,
                    salt,
                    Iterations,
                    HashAlgorithm);

                byte[] keyToCheck = algorithm.GetBytes(KeySize);

                // Compare keys
                return CryptographicOperations.FixedTimeEquals(key, keyToCheck);
            }
            catch
            {
                return false;
            }
        }
    }
}
