namespace Auth.Domain.Entities
{
    public class PasswordResetToken
    {
        public Guid Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime ExpirationDate { get; set; }
    }
}
