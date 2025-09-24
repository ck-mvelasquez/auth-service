namespace Auth.Application.Interfaces
{
    public interface IPasswordResetTokenGenerator
    {
        string GenerateToken();
    }
}
