namespace api.Helpers;

using System.Security.Cryptography;
using api.Helpers.Instances;

public class PasswordHasher : IHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 20000;

    public string HashPassword(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        byte[] salt = new byte[SaltSize];
        rng.GetBytes(salt);
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256
        );
        byte[] key = pbkdf2.GetBytes(KeySize);
        byte[] hashBytes = new byte[SaltSize + KeySize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(key, 0, hashBytes, SaltSize, KeySize);
        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        byte[] hashBytes = Convert.FromBase64String(hashedPassword);
        byte[] salt = new byte[SaltSize];
        Array.Copy(hashBytes, 0, salt, 0, SaltSize);
        byte[] key = new byte[KeySize];
        Array.Copy(hashBytes, SaltSize, key, 0, KeySize);
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256
        );
        byte[] computedKey = pbkdf2.GetBytes(KeySize);
        return CryptographicOperations.FixedTimeEquals(computedKey, key);
    }
}
