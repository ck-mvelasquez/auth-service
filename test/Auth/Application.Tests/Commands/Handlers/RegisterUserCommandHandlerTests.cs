
using Auth.Application.Commands;
using Auth.Application.Commands.Handlers;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Events;
using Auth.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace Auth.Application.Tests.Commands.Handlers;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IEventPublisher> _eventPublisherMock = new();
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        var loggerMock = new Mock<ILogger<RegisterUserCommandHandler>>();
        _handler = new RegisterUserCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _eventPublisherMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_User_And_Publish_Event()
    {
        // Arrange
        var command = new RegisterUserCommand { Email = "test@example.com", Password = "password" };
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email)).ReturnsAsync((User?)null);
        _passwordHasherMock.Setup(x => x.HashPassword(command.Password)).Returns("hashed_password");

        // Act
        await _handler.Handle(command);

        // Assert
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        _eventPublisherMock.Verify(x => x.PublishAsync(It.IsAny<UserRegisteredEvent>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_Exception_If_User_Exists()
    {
        // Arrange
        var command = new RegisterUserCommand { Email = "test@example.com", Password = "password" };
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email)).ReturnsAsync(new User());

        // Act & Assert
        await Assert.ThrowsAsync<System.Exception>(() => _handler.Handle(command));
    }
}
