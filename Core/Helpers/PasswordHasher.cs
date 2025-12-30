using E_Commerce.Core.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace E_Commerce.Core.Helpers
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;

        public string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));

            // Generate salt
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[SaltSize];
            rng.GetBytes(salt);

            // Hash password with salt
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize
            );

            // Combine salt and hash
            var hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            return Convert.ToBase64String(hashBytes);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
                return false;

            try
            {
                var hashBytes = Convert.FromBase64String(hashedPassword);

                if (hashBytes.Length != SaltSize + HashSize)
                    return false;

                // Extract salt
                var salt = new byte[SaltSize];
                Array.Copy(hashBytes, 0, salt, 0, SaltSize);

                // Compute hash of provided password
                var hash = Rfc2898DeriveBytes.Pbkdf2(
                    Encoding.UTF8.GetBytes(password),
                    salt,
                    Iterations,
                    HashAlgorithmName.SHA256,
                    HashSize
                );

                // Compare hashes
                for (int i = 0; i < HashSize; i++)
                {
                    if (hashBytes[i + SaltSize] != hash[i])
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

