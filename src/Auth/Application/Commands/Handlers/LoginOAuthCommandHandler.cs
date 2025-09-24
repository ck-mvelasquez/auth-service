using Auth.Application.Factories;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Events;
using Auth.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Auth.Application.Commands.Handlers;

public class LoginOAuthCommandHandler(
    IUserRepository userRepository,
    IJwtTokenGenerator jwtTokenGenerator,
    IOAuthProviderFactory oauthProviderFactory,
    IEventPublisher eventPublisher,
    ILogger<LoginOAuthCommandHandler> logger)
{
    public async Task<string> Handle(LoginOAuthCommand command)
    {
        logger.LogInformation("Attempting OAuth login for provider: {Provider}", command.Provider);
        var providerService = oauthProviderFactory.GetProvider(command.Provider);
        var userInfo = await providerService.GetUserInfoAsync(command.Provider, command.Code);

        var user = await userRepository.GetByProviderAsync(command.Provider, userInfo.ProviderUserId);

        if (user == null)
        {
            user = new User
            {
                Email = userInfo.Email,
                FullName = userInfo.FullName,
                Provider = command.Provider,
                ProviderUserId = userInfo.ProviderUserId,
            };
            await userRepository.AddAsync(user);
            await eventPublisher.PublishAsync(new UserRegisteredEvent(user.Id, user.Email));
        }
        else
        {
            if (!user.IsActive)
            {
                logger.LogWarning("Inactive user attempted to log in via OAuth: {Email}", user.Email);
                throw new Exception("User account is inactive.");
            }
        }

        var token = jwtTokenGenerator.GenerateToken(user);
        await eventPublisher.PublishAsync(new UserLoggedInEvent(user.Id, user.Email));
        return token;
    }
}
