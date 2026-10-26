using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MockIdentityProvider.Infrastructure.Security;

namespace MockIdentityProvider.Controllers
{
    [ApiController]
    public class JwksController : Controller
    {
        [HttpGet("/.well-known/jwks.json")]
        public IActionResult GetKeys()
        {
            var rsa = RsaKeyProvider.Instance;
            var parameters = rsa.ExportParameters(includePrivateParameters: false);

            var jwk = new
            {
                kty = "RSA",
                use = "sig",
                alg = "RS256",
                kid = RsaKeyProvider.KeyId,
                n = Base64UrlEncoder.Encode(parameters.Modulus!),
                e = Base64UrlEncoder.Encode(parameters.Exponent!)
            };

            return Ok(new { keys = new[] { jwk } });
        }
    }
}
