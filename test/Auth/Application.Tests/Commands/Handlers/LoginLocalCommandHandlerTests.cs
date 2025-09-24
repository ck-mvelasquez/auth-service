
using Auth.Application.Commands;
using Auth.Application.Commands.Handlers;
using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Events;
using Auth.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace Auth.Application.Tests.Commands.Handlers;

public class LoginLocalCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Mock<IEventPublisher> _eventPublisherMock = new();
    private readonly LoginLocalCommandHandler _handler;

    public LoginLocalCommandHandlerTests()
    {
        var loggerMock = new Mock<ILogger<LoginLocalCommandHandler>>();

        _handler = new LoginLocalCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object,
            _refreshTokenRepositoryMock.Object,
            _eventPublisherMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnAuthResult()
    {
        // Arrange
        var command = new LoginLocalCommand { Email = "test@example.com", Password = "password" };
        var user = new User { Email = command.Email, PasswordHash = "hashed_password", IsActive = true };
        var authResult = new AuthResult { Token = "jwt_token", RefreshToken = "refresh_token" };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(command.Email)).ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.VerifyPassword(command.Password, user.PasswordHash)).Returns(true);
        _jwtTokenGeneratorMock.Setup(g => g.GenerateToken(user)).Returns(authResult.Token);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        Assert.Equal(authResult.Token, result.Token);
        Assert.NotNull(result.RefreshToken);
        _eventPublisherMock.Verify(p => p.PublishAsync(It.IsAny<UserLoggedInEvent>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldThrowException()
    {
        // Arrange
        var command = new LoginLocalCommand { Email = "test@example.com", Password = "password" };
        var user = new User { Email = command.Email, PasswordHash = "hashed_password", IsActive = false };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(command.Email)).ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.VerifyPassword(command.Password, user.PasswordHash)).Returns(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command));
        Assert.Equal("User account is inactive.", exception.Message);
    }
}
