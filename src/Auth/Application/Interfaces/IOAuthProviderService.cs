using Auth.Application.DTOs;

namespace Auth.Application.Interfaces;

public interface IOAuthProviderService
{
    Task<OAuthUserInfo> GetUserInfoAsync(string provider, string token);
}
