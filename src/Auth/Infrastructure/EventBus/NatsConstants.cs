
namespace Auth.Infrastructure.EventBus
{
    public static class NatsConstants
    {
        public const string StreamName = "AUTH_EVENTS";

        public static readonly string[] Subjects =
        [
            "UserRegisteredEvent", 
            "UserLoggedInEvent", 
            "OAuthProviderLinkedEvent", 
            "UserStatusChangedEvent",
            "PasswordResetRequestedEvent"
        ];
    }
}
