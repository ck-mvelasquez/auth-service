
using Auth.Application.Commands;
using Auth.Application.Commands.Handlers;
using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Moq;

namespace Auth.Application.Tests.Commands.Handlers;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _handler = new RefreshTokenCommandHandler(
            _refreshTokenRepositoryMock.Object,
            _userRepositoryMock.Object,
            _jwtTokenGeneratorMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidTokenAndActiveUser_ShouldReturnNewTokens()
    {
        // Arrange
        var command = new RefreshTokenCommand { RefreshToken = "valid_refresh_token" };
        var refreshToken = new RefreshToken { Token = command.RefreshToken, UserId = Guid.NewGuid(), ExpiryDate = DateTime.UtcNow.AddDays(1) };
        var user = new User { Id = refreshToken.UserId, IsActive = true };
        var authResult = new AuthResult { Token = "new_jwt", RefreshToken = "new_refresh_token" };

        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync(command.RefreshToken)).ReturnsAsync(refreshToken);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(refreshToken.UserId)).ReturnsAsync(user);
        _jwtTokenGeneratorMock.Setup(g => g.GenerateToken(user)).Returns(authResult.Token);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        Assert.Equal(authResult.Token, result.Token);
        Assert.NotNull(result.RefreshToken);
        _refreshTokenRepositoryMock.Verify(r => r.DeleteAsync(command.RefreshToken), Times.Once);
        _refreshTokenRepositoryMock.Verify(r => r.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldThrowException()
    {
        // Arrange
        var command = new RefreshTokenCommand { RefreshToken = "valid_refresh_token" };
        var refreshToken = new RefreshToken { Token = command.RefreshToken, UserId = Guid.NewGuid(), ExpiryDate = DateTime.UtcNow.AddDays(1) };
        var user = new User { Id = refreshToken.UserId, IsActive = false };

        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync(command.RefreshToken)).ReturnsAsync(refreshToken);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(refreshToken.UserId)).ReturnsAsync(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command));
        Assert.Equal("User account is inactive.", exception.Message);
    }
}
