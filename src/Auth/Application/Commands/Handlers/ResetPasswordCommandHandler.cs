
using Auth.Application.Commands;
using Auth.Application.Interfaces;
using Auth.Domain.Interfaces;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Auth.Application.Commands.Handlers;

public class ResetPasswordCommandHandler(
    IPasswordResetTokenRepository tokenRepository,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ILogger<ResetPasswordCommandHandler> logger)
{
    public async Task Handle(ResetPasswordCommand command)
    {
        logger.LogInformation("Attempting to reset password with token: {Token}", command.Token);

        var resetToken = await tokenRepository.GetByTokenAsync(command.Token);

        if (resetToken == null)
        {
            logger.LogError("Password reset token not found in repository: {Token}", command.Token);
            throw new ApplicationException("Invalid password reset token.");
        }
        
        logger.LogInformation("Found token in repository for email {Email} with expiration {ExpirationDate}", resetToken.Email, resetToken.ExpirationDate);

        if (resetToken.ExpirationDate < DateTime.UtcNow)
        {
            logger.LogWarning("Expired password reset token used for email: {Email}", resetToken.Email);
            await tokenRepository.DeleteAsync(resetToken.Token);
            throw new ApplicationException("Password reset token has expired.");
        }

        var user = await userRepository.GetByEmailAsync(resetToken.Email);
        if (user == null)
        {
            logger.LogError("User not found for email associated with token: {Email}", resetToken.Email);
            throw new ApplicationException("User not found.");
        }

        user.PasswordHash = passwordHasher.HashPassword(command.NewPassword);
        await userRepository.UpdateAsync(user);

        await tokenRepository.DeleteAsync(resetToken.Token);

        logger.LogInformation("Successfully reset password for user: {Email}", user.Email);
    }
}
