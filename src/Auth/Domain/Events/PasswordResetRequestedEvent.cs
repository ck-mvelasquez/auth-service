
namespace Auth.Domain.Events
{
    public class PasswordResetRequestedEvent(string email, string token) : DomainEvent
    {
        public string Email { get; } = email;
        public string Token { get; } = token;
    }
}
