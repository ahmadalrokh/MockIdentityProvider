using Microsoft.AspNetCore.Mvc;

namespace MockIdentityProvider.Domain.Models
{
    public class AuthorizeRequest
    {
        [FromQuery(Name = "client_id")]
        public string ClientId { get; set; } = string.Empty;

        [FromQuery(Name = "redirect_uri")]
        public string RedirectUri { get; set; } = string.Empty;

        [FromQuery(Name = "state")]
        public string State { get; set; } = string.Empty;

        [FromQuery(Name = "nonce")]
        public string? Nonce { get; set; }

        [FromQuery(Name = "response_type")]
        public string ResponseType { get; set; } = "code";

        [FromQuery(Name = "scope")]
        public string Scope { get; set; } = "openid";

        [FromQuery(Name = "code_challenge")]
        public string? CodeChallenge { get; set; }

        [FromQuery(Name = "code_challenge_method")]
        public string? CodeChallengeMethod { get; set; }
    }
}
