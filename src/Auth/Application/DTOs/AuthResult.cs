
namespace Auth.Application.DTOs
{
    public class AuthResult
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
