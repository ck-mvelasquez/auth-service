namespace Auth.Domain.Events;

public abstract class DomainEvent
{
    public DateTimeOffset DateOccurred { get; protected set; } = DateTimeOffset.UtcNow;
}
