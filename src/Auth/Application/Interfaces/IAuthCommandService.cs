
using Auth.Application.Commands;
using Auth.Application.DTOs;
using System.Threading.Tasks;

namespace Auth.Application.Interfaces
{
    public interface IAuthCommandService
    {
        Task RegisterUserAsync(RegisterUserCommand command);
        Task<AuthResult> LoginLocalAsync(LoginLocalCommand command);
        Task<AuthResult> RefreshTokenAsync(RefreshTokenCommand command);
        Task ForgotPasswordAsync(ForgotPasswordCommand command);
        Task ResetPasswordAsync(ResetPasswordCommand command);
    }
}
