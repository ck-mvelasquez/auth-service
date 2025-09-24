
using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Events;
using Auth.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace Auth.Application.Commands.Handlers;

public class LoginLocalCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenRepository refreshTokenRepository,
    IEventPublisher eventPublisher,
    ILogger<LoginLocalCommandHandler> logger)
{
    public async Task<AuthResult> Handle(LoginLocalCommand command)
    {
        logger.LogInformation("Attempting to login user with email {Email}", command.Email);

        var user = await userRepository.GetByEmailAsync(command.Email);
        if (user is null || user.PasswordHash is null || !passwordHasher.VerifyPassword(command.Password, user.PasswordHash))
        {
            logger.LogWarning("Invalid email or password for user {Email}", command.Email);
            throw new Exception("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            logger.LogWarning("Inactive user attempted to log in: {Email}", command.Email);
            throw new Exception("User account is inactive.");
        }

        var accessToken = jwtTokenGenerator.GenerateToken(user);

        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(7)
        };
        await refreshTokenRepository.AddAsync(refreshToken);

        // Use the new, secure event with only the necessary data
        await eventPublisher.PublishAsync(new UserLoggedInEvent(user.Id, user.Email));

        logger.LogInformation("User {Email} logged in successfully", command.Email);

        return new AuthResult { Token = accessToken, RefreshToken = refreshToken.Token };
    }
}
