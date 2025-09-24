using Auth.Domain.Entities;

namespace Auth.Domain.Events;

public class OAuthProviderLinkedEvent(User user, string provider) : DomainEvent
{
    public User User { get; } = user;
    public string Provider { get; } = provider;
}
