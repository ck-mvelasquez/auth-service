using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

namespace Auth.Infrastructure.AuthProviders
{
    public class GoogleAuthProviderService(IConfiguration configuration) : IOAuthProviderService
    {
        private readonly IConfiguration _configuration = configuration;

        public async Task<OAuthUserInfo> GetUserInfoAsync(string provider, string token)
        {
            var googleClientId = _configuration["Google:ClientId"];
            if (string.IsNullOrEmpty(googleClientId))
            {
                throw new Exception("Google Client ID is not configured.");
            }

            var payload = await GoogleJsonWebSignature.ValidateAsync(token, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [googleClientId]
            });

            return new OAuthUserInfo
            {
                Email = payload.Email,
                ProviderUserId = payload.Subject
            };
        }
    }
}
