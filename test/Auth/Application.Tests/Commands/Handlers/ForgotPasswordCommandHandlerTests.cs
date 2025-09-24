using Auth.Application.Commands;
using Auth.Application.Commands.Handlers;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace Auth.Application.Tests.Commands.Handlers
{
    public class ForgotPasswordCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPasswordResetTokenRepository> _tokenRepositoryMock;
        private readonly Mock<IPasswordResetTokenGenerator> _tokenGeneratorMock;
        private readonly Mock<IEventPublisher> _eventPublisherMock;
        private readonly ForgotPasswordCommandHandler _handler;

        public ForgotPasswordCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _tokenRepositoryMock = new Mock<IPasswordResetTokenRepository>();
            _tokenGeneratorMock = new Mock<IPasswordResetTokenGenerator>();
            _eventPublisherMock = new Mock<IEventPublisher>();

            // Correctly inject the mock logger
            _handler = new ForgotPasswordCommandHandler(
                _userRepositoryMock.Object,
                _tokenRepositoryMock.Object,
                _tokenGeneratorMock.Object,
                _eventPublisherMock.Object,
                new Mock<ILogger<ForgotPasswordCommandHandler>>().Object);
        }

        [Fact]
        public async Task Handle_Should_Generate_And_Save_Token_When_User_Exists()
        {
            // Arrange
            var command = new ForgotPasswordCommand { Email = "test@example.com" };
            _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email)).ReturnsAsync(new User());
            _tokenGeneratorMock.Setup(x => x.GenerateToken()).Returns("reset_token");

            // Act
            await _handler.Handle(command);

            // Assert
            _tokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<PasswordResetToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Not_Generate_Token_When_User_Does_Not_Exist()
        {
            // Arrange
            var command = new ForgotPasswordCommand { Email = "test@example.com" };
            _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email)).ReturnsAsync(default(User));

            // Act
            await _handler.Handle(command);

            // Assert
            _tokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<PasswordResetToken>()), Times.Never);
        }
    }
}
