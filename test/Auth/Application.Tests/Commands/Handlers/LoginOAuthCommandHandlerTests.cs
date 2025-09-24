
using Auth.Application.Commands;
using Auth.Application.Commands.Handlers;
using Auth.Application.DTOs;
using Auth.Application.Factories;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Events;
using Auth.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace Auth.Application.Tests.Commands.Handlers;

public class LoginOAuthCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IOAuthProviderFactory> _oauthProviderFactoryMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
    private readonly Mock<IEventPublisher> _eventPublisherMock = new();
    private readonly Mock<IOAuthProviderService> _oauthProviderServiceMock = new();
    private readonly LoginOAuthCommandHandler _handler;

    public LoginOAuthCommandHandlerTests()
    {
        var loggerMock = new Mock<ILogger<LoginOAuthCommandHandler>>();

        _oauthProviderFactoryMock.Setup(f => f.GetProvider(It.IsAny<string>())).Returns(_oauthProviderServiceMock.Object);

        _handler = new LoginOAuthCommandHandler(
            _userRepositoryMock.Object,
            _jwtTokenGeneratorMock.Object,
            _oauthProviderFactoryMock.Object,
            _eventPublisherMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingActiveUser_ShouldReturnToken()
    {
        // Arrange
        var command = new LoginOAuthCommand { Provider = "Google", Code = "auth_code" };
        var userInfo = new OAuthUserInfo { ProviderUserId = "123", Email = "test@example.com", FullName = "Test User" };
        var user = new User { Provider = "Google", ProviderUserId = "123", IsActive = true };
        var token = "jwt_token";

        _oauthProviderServiceMock.Setup(s => s.GetUserInfoAsync(command.Provider, command.Code)).ReturnsAsync(userInfo);
        _userRepositoryMock.Setup(r => r.GetByProviderAsync(command.Provider, userInfo.ProviderUserId)).ReturnsAsync(user);
        _jwtTokenGeneratorMock.Setup(g => g.GenerateToken(user)).Returns(token);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        Assert.Equal(token, result);
        _eventPublisherMock.Verify(p => p.PublishAsync(It.IsAny<UserLoggedInEvent>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingInactiveUser_ShouldThrowException()
    {
        // Arrange
        var command = new LoginOAuthCommand { Provider = "Google", Code = "auth_code" };
        var userInfo = new OAuthUserInfo { ProviderUserId = "123", Email = "test@example.com", FullName = "Test User" };
        var user = new User { Provider = "Google", ProviderUserId = "123", IsActive = false };

        _oauthProviderServiceMock.Setup(s => s.GetUserInfoAsync(command.Provider, command.Code)).ReturnsAsync(userInfo);
        _userRepositoryMock.Setup(r => r.GetByProviderAsync(command.Provider, userInfo.ProviderUserId)).ReturnsAsync(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command));
        Assert.Equal("User account is inactive.", exception.Message);
    }

    [Fact]
    public async Task Handle_WithNewUser_ShouldCreateUserAndReturnToken()
    {
        // Arrange
        var command = new LoginOAuthCommand { Provider = "Google", Code = "auth_code" };
        var userInfo = new OAuthUserInfo { ProviderUserId = "123", Email = "test@example.com", FullName = "Test User" };
        var token = "jwt_token";

        _oauthProviderServiceMock.Setup(s => s.GetUserInfoAsync(command.Provider, command.Code)).ReturnsAsync(userInfo);
        _userRepositoryMock.Setup(r => r.GetByProviderAsync(command.Provider, userInfo.ProviderUserId)).ReturnsAsync((User?)null);
        _jwtTokenGeneratorMock.Setup(g => g.GenerateToken(It.IsAny<User>())).Returns(token);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        Assert.Equal(token, result);
        _userRepositoryMock.Verify(r => r.AddAsync(It.Is<User>(u => u.ProviderUserId == userInfo.ProviderUserId && u.IsActive)), Times.Once);
        _eventPublisherMock.Verify(p => p.PublishAsync(It.IsAny<UserRegisteredEvent>()), Times.Once);
        _eventPublisherMock.Verify(p => p.PublishAsync(It.IsAny<UserLoggedInEvent>()), Times.Once);
    }
}
