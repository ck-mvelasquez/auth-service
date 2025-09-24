
namespace Auth.Domain.Events
{
    public class UserRegisteredEvent(Guid userId, string email) : DomainEvent
    {
        public Guid UserId { get; } = userId;
        public string Email { get; } = email;
    }
}
