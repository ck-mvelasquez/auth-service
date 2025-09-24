
using Auth.Api.DTOs;
using Auth.Application.Commands;
using Auth.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthCommandService authCommandService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var command = new RegisterUserCommand { Email = request.Email, Password = request.Password };
        await authCommandService.RegisterUserAsync(command);
        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginLocalCommand { Email = request.Email, Password = request.Password };
        var result = await authCommandService.LoginLocalAsync(command);
        return Ok(new AuthResponse { Token = result.Token, RefreshToken = result.RefreshToken });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand { RefreshToken = request.RefreshToken };
        var result = await authCommandService.RefreshTokenAsync(command);
        return Ok(new AuthResponse { Token = result.Token, RefreshToken = result.RefreshToken });
    }

    [Authorize]
    [HttpGet("validate")]
    public IActionResult ValidateToken()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value });
        return Ok(claims);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var command = new ForgotPasswordCommand { Email = request.Email };
        await authCommandService.ForgotPasswordAsync(command);
        return Ok();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var command = new ResetPasswordCommand { Email = request.Email, Token = request.Token, NewPassword = request.NewPassword };
        await authCommandService.ResetPasswordAsync(command);
        return Ok();
    }
}
