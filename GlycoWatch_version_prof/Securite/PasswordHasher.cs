using System.Security.Cryptography;

namespace GlycoWatch_version_prof.Security
{
    public static class PasswordHasher
    {
        public static void CreateHash(string password, out byte[] hash, out byte[] salt)
        {
            salt = new byte[16];
            RandomNumberGenerator.Fill(salt);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            hash = pbkdf2.GetBytes(64);
        }

        public static bool Verify(string password, byte[] salt, byte[] expectedHash)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            var h = pbkdf2.GetBytes(64);
            return CryptographicOperations.FixedTimeEquals(h, expectedHash);
        }
    }
}
