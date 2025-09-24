using Auth.Application.Interfaces;
using System.Security.Cryptography;

namespace Auth.Infrastructure.Security
{
    public class PasswordResetTokenGenerator : IPasswordResetTokenGenerator
    {
        public string GenerateToken()
        {
            // Generate a secure random token
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
