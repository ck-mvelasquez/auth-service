using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Octokit;

namespace Auth.Infrastructure.AuthProviders
{
    public class GitHubAuthProviderService : IOAuthProviderService
    {
        public async Task<OAuthUserInfo> GetUserInfoAsync(string provider, string token)
        {
            var github = new GitHubClient(new ProductHeaderValue("auth-service"))
            {
                Credentials = new Credentials(token)
            };
            var user = await github.User.Current();

            return new OAuthUserInfo
            {
                Email = user.Email,
                ProviderUserId = user.Id.ToString()
            };
        }
    }
}
