using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Auth.Infrastructure.Security
{
    public interface ISigningKeyService
    {
        RsaSecurityKey GetRsaSecurityKey();
        JsonWebKey GetJsonWebKey();
    }

    public class SigningKeyService : ISigningKeyService
    {
        private readonly RsaSecurityKey _key;
        private readonly JsonWebKey _jsonWebKey;

        public SigningKeyService()
        {
            var rsa = RSA.Create();
            _key = new RsaSecurityKey(rsa) { KeyId = "auth-key" };

            var parameters = rsa.ExportParameters(false);
            _jsonWebKey = new JsonWebKey
            {
                Kty = "RSA",
                Kid = _key.KeyId,
                Use = "sig",
                E = Base64UrlEncoder.Encode(parameters.Exponent),
                N = Base64UrlEncoder.Encode(parameters.Modulus)
            };
        }

        public RsaSecurityKey GetRsaSecurityKey() => _key;
        public JsonWebKey GetJsonWebKey() => _jsonWebKey;
    }
}
