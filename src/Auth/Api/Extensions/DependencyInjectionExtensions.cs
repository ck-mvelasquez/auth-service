
using Auth.Application.Commands.Handlers;
using Auth.Application.Factories;
using Auth.Application.Interfaces;
using Auth.Application.Services;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.AuthProviders;
using Auth.Infrastructure.Factories;
using Auth.Infrastructure.Persistence.Repositories;
using Auth.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Auth.Api.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPasswordResetTokenGenerator, PasswordResetTokenGenerator>();
        services.AddScoped<GoogleAuthProviderService>();
        services.AddScoped<GitHubAuthProviderService>();
        services.AddScoped<IOAuthProviderFactory, OAuthProviderFactory>();

        services.AddScoped<RegisterUserCommandHandler>();
        services.AddScoped<LoginLocalCommandHandler>();
        services.AddScoped<RefreshTokenCommandHandler>();
        services.AddScoped<LoginOAuthCommandHandler>();
        services.AddScoped<LinkProviderCommandHandler>();
        
        // Updated registration for ForgotPasswordCommandHandler
        services.AddScoped(provider => new ForgotPasswordCommandHandler(
            provider.GetRequiredService<IUserRepository>(),
            provider.GetRequiredService<IPasswordResetTokenRepository>(),
            provider.GetRequiredService<IPasswordResetTokenGenerator>(),
            provider.GetRequiredService<IEventPublisher>(),
            provider.GetRequiredService<ILogger<ForgotPasswordCommandHandler>>()
        ));

        services.AddScoped<ResetPasswordCommandHandler>();

        services.AddSingleton<ISigningKeyService, SigningKeyService>();
        services.AddScoped<IAuthCommandService, AuthCommandService>();

        return services;
    }
}
