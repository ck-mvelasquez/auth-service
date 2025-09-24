using Auth.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Api.Controllers
{
    [ApiController]
    [Route(".well-known")]
    public class JwksController(ISigningKeyService signingKeyService) : ControllerBase
    {
        [HttpGet("jwks.json")]
        [AllowAnonymous]
        public IActionResult GetJwks()
        {
            var jwk = signingKeyService.GetJsonWebKey();
            var jwks = new JsonWebKeySet();
            jwks.Keys.Add(jwk);
            return Ok(jwks);
        }

        [HttpGet("openid-configuration")]
        [AllowAnonymous]
        public IActionResult GetOpenIdConfiguration()
        {
            var issuer = $"http://{Request.Host}";
            var jwksUri = $"{issuer}/.well-known/jwks.json";

            var configuration = new
            {
                issuer,
                jwks_uri = jwksUri,
                // Add other OpenID configuration properties as needed
            };

            return Ok(configuration);
        }
    }
}
