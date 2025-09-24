namespace Auth.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? PasswordHash { get; set; }
        public string? FullName { get; set; }
        public bool IsActive { get; set; } = true;

        public string? Provider { get; set; }
        public string? ProviderUserId { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    }
}
