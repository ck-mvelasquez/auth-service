using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Auth.Infrastructure.Security
{
    public class JwtTokenGenerator(IConfiguration configuration, ISigningKeyService signingKeyService) : IJwtTokenGenerator
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly ISigningKeyService _signingKeyService = signingKeyService;

        public string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            };

            var credentials = new SigningCredentials(_signingKeyService.GetRsaSecurityKey(), SecurityAlgorithms.RsaSha256);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: System.DateTime.UtcNow.AddMinutes(60),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
