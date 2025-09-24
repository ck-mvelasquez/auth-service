
using Auth.Application.Commands;
using Auth.Application.Interfaces;
using Auth.Infrastructure.Security;
using AuthApi;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Api.Services
{
    public class GrpcAuthService(
        IAuthCommandService authCommandService,
        ILogger<GrpcAuthService> logger,
        ISigningKeyService signingKeyService) : AuthService.AuthServiceBase
    {
        public override async Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
        {
            logger.LogInformation("gRPC-Register request received for {Email}", request.Email);
            var command = new RegisterUserCommand { Email = request.Email, Password = request.Password };
            await authCommandService.RegisterUserAsync(command);
            return new RegisterResponse();
        }

        public override async Task<AuthResponse> Login(LoginRequest request, ServerCallContext context)
        {
            logger.LogInformation("gRPC-Login request received for {Email}", request.Email);
            var command = new LoginLocalCommand { Email = request.Email, Password = request.Password };
            var result = await authCommandService.LoginLocalAsync(command);
            return new AuthResponse { Token = result.Token, RefreshToken = result.RefreshToken };
        }

        public override async Task<AuthResponse> RefreshToken(RefreshTokenRequest request, ServerCallContext context)
        {
            logger.LogInformation("gRPC-RefreshToken request received");
            var command = new RefreshTokenCommand { RefreshToken = request.RefreshToken };
            var result = await authCommandService.RefreshTokenAsync(command);
            return new AuthResponse { Token = result.Token, RefreshToken = result.RefreshToken };
        }

        public override async Task<ForgotPasswordResponse> ForgotPassword(ForgotPasswordRequest request, ServerCallContext context)
        {
            logger.LogInformation("gRPC-ForgotPassword request received for {Email}", request.Email);
            var command = new ForgotPasswordCommand { Email = request.Email };
            await authCommandService.ForgotPasswordAsync(command);
            return new ForgotPasswordResponse();
        }

        public override async Task<ResetPasswordResponse> ResetPassword(ResetPasswordRequest request, ServerCallContext context)
        {
            logger.LogInformation("gRPC-ResetPassword request received");
            var command = new ResetPasswordCommand { Token = request.Token, NewPassword = request.NewPassword };
            await authCommandService.ResetPasswordAsync(command);
            return new ResetPasswordResponse();
        }

        [Authorize]
        public override Task<ValidateTokenResponse> ValidateToken(ValidateTokenRequest request, ServerCallContext context)
        {
            logger.LogInformation("gRPC-ValidateToken request received");
            return Task.FromResult(new ValidateTokenResponse { IsValid = true });
        }

        public override Task<GetJwksResponse> GetJwks(GetJwksRequest request, ServerCallContext context)
        {
            logger.LogInformation("gRPC-GetJwks request received");
            var jwk = signingKeyService.GetJsonWebKey();
            var jwks = new JsonWebKeySet();
            jwks.Keys.Add(jwk);
            var jwksJson = System.Text.Json.JsonSerializer.Serialize(jwks);
            return Task.FromResult(new GetJwksResponse { Jwks = jwksJson });
        }
    }
}
