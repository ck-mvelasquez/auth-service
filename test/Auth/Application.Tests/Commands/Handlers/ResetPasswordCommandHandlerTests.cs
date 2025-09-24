
using Auth.Application.Commands;
using Auth.Application.Commands.Handlers;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace Auth.Application.Tests.Commands.Handlers;

public class ResetPasswordCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPasswordResetTokenRepository> _tokenRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly ResetPasswordCommandHandler _handler;

    public ResetPasswordCommandHandlerTests()
    {
        var loggerMock = new Mock<ILogger<ResetPasswordCommandHandler>>();

        _handler = new ResetPasswordCommandHandler(
            _tokenRepositoryMock.Object,
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldResetPassword()
    {
        // Arrange
        var command = new ResetPasswordCommand { Token = "valid_token", NewPassword = "new_password" };
        var resetToken = new PasswordResetToken { Token = command.Token, Email = "test@example.com", ExpirationDate = DateTime.UtcNow.AddHours(1) };
        var user = new User { Email = "test@example.com" };
        var hashedPassword = "hashed_new_password";

        _tokenRepositoryMock.Setup(r => r.GetByTokenAsync(command.Token)).ReturnsAsync(resetToken);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(resetToken.Email)).ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.HashPassword(command.NewPassword)).Returns(hashedPassword);

        // Act
        await _handler.Handle(command);

        // Assert
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.Is<User>(u => u.PasswordHash == hashedPassword)), Times.Once);
        _tokenRepositoryMock.Verify(r => r.DeleteAsync(command.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidToken_ShouldThrowException()
    {
        // Arrange
        var command = new ResetPasswordCommand { Token = "invalid_token", NewPassword = "new_password" };
        _tokenRepositoryMock.Setup(r => r.GetByTokenAsync(command.Token)).ReturnsAsync((PasswordResetToken?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(() => _handler.Handle(command));
        Assert.Equal("Invalid password reset token.", exception.Message);
    }

    [Fact]
    public async Task Handle_WithExpiredToken_ShouldThrowExceptionAndDeletetoken()
    {
        // Arrange
        var command = new ResetPasswordCommand { Token = "expired_token", NewPassword = "new_password" };
        var resetToken = new PasswordResetToken { Token = command.Token, Email = "test@example.com", ExpirationDate = DateTime.UtcNow.AddHours(-1) };

        _tokenRepositoryMock.Setup(r => r.GetByTokenAsync(command.Token)).ReturnsAsync(resetToken);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(() => _handler.Handle(command));
        Assert.Equal("Password reset token has expired.", exception.Message);
        _tokenRepositoryMock.Verify(r => r.DeleteAsync(command.Token), Times.Once);
    }
}
