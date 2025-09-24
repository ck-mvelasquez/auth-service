using Auth.Domain.Events;

namespace Auth.Application.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync(DomainEvent domainEvent);
}
