using Auth.Api.Services;
using Auth.Application.Commands;
using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Infrastructure.Security;
using AuthApi;
using Grpc.Core;
using Grpc.Core.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.Text.Json;

namespace Auth.Api.Tests
{
    public class GrpcAuthServiceTests
    {
        private readonly Mock<IAuthCommandService> _authCommandServiceMock = new();
        private readonly Mock<ILogger<GrpcAuthService>> _loggerMock = new();
        private readonly Mock<ISigningKeyService> _signingKeyServiceMock = new();
        private readonly GrpcAuthService _service;
        private readonly ServerCallContext _context;

        public GrpcAuthServiceTests()
        {
            _service = new GrpcAuthService(_authCommandServiceMock.Object, _loggerMock.Object, _signingKeyServiceMock.Object);
            _context = TestServerCallContext.Create("test", null, DateTime.UtcNow, [], CancellationToken.None, "peer", null, null, null, null, null);
        }

        [Fact]
        public async Task Register_ShouldCallCommandService()
        {
            // Arrange
            var request = new RegisterRequest { Email = "test@example.com", Password = "password" };

            // Act
            var response = await _service.Register(request, _context);

            // Assert
            _authCommandServiceMock.Verify(s => s.RegisterUserAsync(It.Is<RegisterUserCommand>(c => c.Email == request.Email)), Times.Once);
            Assert.IsType<RegisterResponse>(response);
        }

        [Fact]
        public async Task Login_ShouldCallCommandServiceAndReturnAuthResponse()
        {
            // Arrange
            var request = new LoginRequest { Email = "test@example.com", Password = "password" };
            var authResult = new AuthResult { Token = "jwt", RefreshToken = "refresh" };
            _authCommandServiceMock.Setup(s => s.LoginLocalAsync(It.IsAny<LoginLocalCommand>())).ReturnsAsync(authResult);

            // Act
            var response = await _service.Login(request, _context);

            // Assert
            _authCommandServiceMock.Verify(s => s.LoginLocalAsync(It.Is<LoginLocalCommand>(c => c.Email == request.Email)), Times.Once);
            Assert.Equal(authResult.Token, response.Token);
            Assert.Equal(authResult.RefreshToken, response.RefreshToken);
        }

        [Fact]
        public async Task RefreshToken_ShouldCallCommandService()
        {
            // Arrange
            var request = new RefreshTokenRequest { RefreshToken = "refresh" };
            var authResult = new AuthResult { Token = "new_jwt", RefreshToken = "new_refresh" };
            _authCommandServiceMock.Setup(s => s.RefreshTokenAsync(It.IsAny<RefreshTokenCommand>())).ReturnsAsync(authResult);

            // Act
            var response = await _service.RefreshToken(request, _context);

            // Assert
            _authCommandServiceMock.Verify(s => s.RefreshTokenAsync(It.Is<RefreshTokenCommand>(c => c.RefreshToken == request.RefreshToken)), Times.Once);
            Assert.Equal(authResult.Token, response.Token);
        }

        [Fact]
        public async Task ValidateToken_ShouldReturnIsValidTrue()
        {
            // Arrange
            var request = new ValidateTokenRequest { Token = "valid-token" };

            // Act
            var response = await _service.ValidateToken(request, _context);

            // Assert
            Assert.True(response.IsValid);
        }

        [Fact]
        public async Task GetJwks_ShouldReturnJwks()
        {
            // Arrange
            var request = new GetJwksRequest();
            var jwk = new JsonWebKey { Kid = "test-key", Kty = "RSA" };
            var jwks = new JsonWebKeySet();
            jwks.Keys.Add(jwk);
            var jwksJson = JsonSerializer.Serialize(jwks);
            _signingKeyServiceMock.Setup(s => s.GetJsonWebKey()).Returns(jwk);

            // Act
            var response = await _service.GetJwks(request, _context);

            // Assert
            Assert.Equal(jwksJson, response.Jwks);
        }
    }
}
