
using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using System.Security.Cryptography;

namespace Auth.Application.Commands.Handlers;

public class RefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUserRepository userRepository,
    IJwtTokenGenerator jwtTokenGenerator)
{
    public async Task<AuthResult> Handle(RefreshTokenCommand command)
    {
        var oldRefreshToken = await refreshTokenRepository.GetByTokenAsync(command.RefreshToken)
            ?? throw new Exception("Invalid or expired refresh token.");

        if (oldRefreshToken.ExpiryDate < DateTime.UtcNow)
        {
            throw new Exception("Invalid or expired refresh token.");
        }

        var user = await userRepository.GetByIdAsync(oldRefreshToken.UserId)
            ?? throw new Exception("User not found.");

        if (!user.IsActive)
        {
            throw new Exception("User account is inactive.");
        }

        await refreshTokenRepository.DeleteAsync(oldRefreshToken.Token);

        var newAccessToken = jwtTokenGenerator.GenerateToken(user);

        var newRefreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(7)
        };
        await refreshTokenRepository.AddAsync(newRefreshToken);

        return new AuthResult { Token = newAccessToken, RefreshToken = newRefreshToken.Token };
    }
}
