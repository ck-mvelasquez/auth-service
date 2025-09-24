
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

public class LinkProviderCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IOAuthProviderFactory> _oauthProviderFactoryMock = new();
    private readonly Mock<IEventPublisher> _eventPublisherMock = new();
    private readonly Mock<IOAuthProviderService> _oauthProviderServiceMock = new();
    private readonly LinkProviderCommandHandler _handler;

    public LinkProviderCommandHandlerTests()
    {
        var loggerMock = new Mock<ILogger<LinkProviderCommandHandler>>();

        _oauthProviderFactoryMock.Setup(f => f.GetProvider(It.IsAny<string>())).Returns(_oauthProviderServiceMock.Object);

        _handler = new LinkProviderCommandHandler(
            _userRepositoryMock.Object,
            _oauthProviderFactoryMock.Object,
            _eventPublisherMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidUser_ShouldLinkProvider()
    {
        // Arrange
        var command = new LinkProviderCommand { UserId = Guid.NewGuid(), Provider = "Google", Token = "auth_code" };
        var userInfo = new OAuthUserInfo { ProviderUserId = "123" };
        var user = new User { Id = command.UserId };

        _oauthProviderServiceMock.Setup(s => s.GetUserInfoAsync(command.Provider, command.Token)).ReturnsAsync(userInfo);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(command.UserId)).ReturnsAsync(user);

        // Act
        await _handler.Handle(command);

        // Assert
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.Is<User>(u =>
            u.Provider == command.Provider &&
            u.ProviderUserId == userInfo.ProviderUserId
        )), Times.Once);

        _eventPublisherMock.Verify(p => p.PublishAsync(It.IsAny<OAuthProviderLinkedEvent>()), Times.Once);
    }
}
