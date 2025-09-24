using Auth.Application.Factories;
using Auth.Application.Interfaces;
using Auth.Infrastructure.AuthProviders;

namespace Auth.Infrastructure.Factories
{
    public class OAuthProviderFactory(IServiceProvider serviceProvider) : IOAuthProviderFactory
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public IOAuthProviderService GetProvider(string provider)
        {
            var service = provider.ToLower() switch
            {
                "google" => _serviceProvider.GetService(typeof(GoogleAuthProviderService)),
                "github" => _serviceProvider.GetService(typeof(GitHubAuthProviderService)),
                _ => throw new NotSupportedException($"Provider '{provider}' is not supported.")
            };

            return service == null
                ? throw new Exception($"Could not resolve an implementation of IOAuthProviderService for the provider '{provider}'.")
                : (IOAuthProviderService)service;
        }
    }
}
