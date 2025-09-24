
namespace Auth.Application.DTOs
{
    public class OAuthUserInfo
    {
        public string ProviderUserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}
