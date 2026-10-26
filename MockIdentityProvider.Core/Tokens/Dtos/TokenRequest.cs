using Microsoft.AspNetCore.Mvc;

namespace MockIdentityProvider.Core.Tokens.Dtos
{
    public class TokenRequest
    {
        [FromForm(Name = "grant_type")]
        public string GrantType { get; set; } = string.Empty;

        [FromForm(Name = "code")]
        public string? Code { get; set; }

        [FromForm(Name = "client_id")]
        public string ClientId { get; set; } = string.Empty;

        [FromForm(Name = "client_secret")]
        public string ClientSecret { get; set; } = string.Empty;

        [FromForm(Name = "redirect_uri")]
        public string RedirectUri { get; set; } = string.Empty;

        [FromForm(Name = "code_verifier")]
        public string? CodeVerifier { get; set; }

        [FromForm(Name = "refresh_token")]
        public string? RefreshToken { get; set; }
    }
}
