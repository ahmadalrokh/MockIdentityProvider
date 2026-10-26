using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MockIdentityProvider.Infrastructure.Configurations;
using MockIdentityProvider.Models;

namespace MockIdentityProvider.Controllers
{
    [ApiController]
    public class OpenIdConfigurationController : Controller
    {
        private readonly JwtSettings _setting;

        public OpenIdConfigurationController(IOptions<JwtSettings> setting)
        {
            _setting = setting.Value;
        }

        [HttpGet("/.well-known/openid-configuration")]
        public IActionResult Get()
        {
            var issuer = _setting.Issuer;

            var result = new OpenIdConfigurationResponse
            {
                Issuer = issuer,
                AuthorizationEndpoint = $"{issuer}/connect/authorize",
                TokenEndpoint = $"{issuer}/connect/token",
                UserInfoEndpoint = $"{issuer}/connect/userinfo",
                EndSessionEndpoint = $"{issuer}/connect/logout",
                JwksUri = $"{issuer}/.well-known/jwks.json",
                ResponseTypesSupported = ["code"],
                GrantTypesSupported = ["authorization_code", "refresh_token", "client_credentials"],
                SubjectTypesSupported = ["public"],
                IdTokenSigningAlgValuesSupported = ["RS256"],
                TokenEndpointAuthMethodsSupported = ["none", "client_secret_post"],
                CodeChallengeMethodsSupported = ["S256"]
            };

            return Ok(result);
        }
    }
}
