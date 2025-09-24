
using Auth.Application.Factories;
using Auth.Application.Interfaces;
using Auth.Domain.Events;
using Auth.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Auth.Application.Commands.Handlers;

public class LinkProviderCommandHandler(
    IUserRepository userRepository,
    IOAuthProviderFactory oauthProviderFactory,
    IEventPublisher eventPublisher,
    ILogger<LinkProviderCommandHandler> logger)
{
    public async Task Handle(LinkProviderCommand command)
    {
        logger.LogInformation("Attempting to link provider {Provider} for user {UserId}", command.Provider, command.UserId);
        var providerService = oauthProviderFactory.GetProvider(command.Provider);
        var userInfo = await providerService.GetUserInfoAsync(command.Provider, command.Token);

        var user = await userRepository.GetByIdAsync(command.UserId)
            ?? throw new Exception("User not found.");

        user.Provider = command.Provider;
        user.ProviderUserId = userInfo.ProviderUserId;
        await userRepository.UpdateAsync(user);

        await eventPublisher.PublishAsync(new OAuthProviderLinkedEvent(user, command.Provider));
    }
}
