
namespace Auth.Domain.Events
{
    public class UserLoggedInEvent(Guid userId, string email) : DomainEvent
    {
        public Guid UserId { get; } = userId;
        public string Email { get; } = email;
    }
}
