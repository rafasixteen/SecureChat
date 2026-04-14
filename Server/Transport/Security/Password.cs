using System.Security.Cryptography;

namespace Server.Transport.Security
{
    public static class Password
    {
        private const int SaltSize = 32;

        private const int HashSize = 32;

        // OWASP recommended minimum for PBKDF2-SHA256
        // TODO: Why this number?
        private const int Iterations = 600_000;

        public static (string hash, string salt) Hash(string password)
        {
            byte[] saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hashBytes = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, Iterations, HashAlgorithmName.SHA256, HashSize);
            return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
        }

        public static bool Verify(string password, string hash, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            byte[] hashBytes = Convert.FromBase64String(hash);
            byte[] computedHashBytes = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, Iterations, HashAlgorithmName.SHA256, HashSize);

            // Constant-time comparison to prevent timing attacks
            // TODO: What is const-time comparison and how does it work?
            return CryptographicOperations.FixedTimeEquals(hashBytes, computedHashBytes);
        }


    }
}
