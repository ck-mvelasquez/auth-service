using System.ComponentModel.DataAnnotations;

namespace Auth.Api.DTOs;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
