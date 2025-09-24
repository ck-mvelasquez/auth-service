
using Auth.Application.Commands;
using Auth.Application.Commands.Handlers;
using Auth.Application.DTOs;
using Auth.Application.Interfaces;

namespace Auth.Application.Services;

public class AuthCommandService(
    RegisterUserCommandHandler registerUserCommandHandler,
    LoginLocalCommandHandler loginLocalCommandHandler,
    RefreshTokenCommandHandler refreshTokenCommandHandler,
    ForgotPasswordCommandHandler forgotPasswordCommandHandler,
    ResetPasswordCommandHandler resetPasswordCommandHandler) : IAuthCommandService
{
    public async Task ForgotPasswordAsync(ForgotPasswordCommand command)
    {
        await forgotPasswordCommandHandler.Handle(command);
    }

    public async Task<AuthResult> LoginLocalAsync(LoginLocalCommand command)
    {
        return await loginLocalCommandHandler.Handle(command);
    }

    public async Task<AuthResult> RefreshTokenAsync(RefreshTokenCommand command)
    {
        return await refreshTokenCommandHandler.Handle(command);
    }

    public async Task RegisterUserAsync(RegisterUserCommand command)
    {
        await registerUserCommandHandler.Handle(command);
    }

    public async Task ResetPasswordAsync(ResetPasswordCommand command)
    {
        await resetPasswordCommandHandler.Handle(command);
    }
}
