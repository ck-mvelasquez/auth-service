using Auth.Application.Interfaces;

namespace Auth.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    // A work factor of 12 is a good balance between security and performance.
    private const int WorkFactor = 12;

    public string HashPassword(string password)
    {
        // Generate a salt and hash the password using SHA256 within the BCrypt algorithm.
        var salt = BCrypt.Net.BCrypt.GenerateSalt(WorkFactor);
        return BCrypt.Net.BCrypt.HashPassword(password, salt, enhancedEntropy: true, hashType: BCrypt.Net.HashType.SHA256);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        // The Verify method automatically determines the hash type and parameters from the hash string.
        return BCrypt.Net.BCrypt.Verify(password, passwordHash, enhancedEntropy: true, hashType: BCrypt.Net.HashType.SHA256);
    }
}
