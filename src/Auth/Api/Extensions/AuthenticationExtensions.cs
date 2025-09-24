
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Api.Extensions
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = "http://localhost:8080";
                    options.Audience = configuration["Jwt:Audience"]!;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = configuration["Jwt:Issuer"]!,
                        ValidateAudience = true,
                        ValidAudience = configuration["Jwt:Audience"]!,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                    };
                    options.RequireHttpsMetadata = false;
                });

            return services;
        }
    }
}
