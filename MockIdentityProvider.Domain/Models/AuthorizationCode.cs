namespace MockIdentityProvider.Domain.Models
{
    public class AuthorizationCode
    {
        public string Code { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public string Realm { get; set; } = default!;
        public string ClientId { get; set; } = default!;
        public string RedirectUri { get; set; } = string.Empty;
        public string? Nonce { get; set; }
        public string? CodeChallenge { get; set; }
        public string? CodeChallengeMethod { get; set; }

        public DateTime ExpiresAt { get; set; }
        public bool Used { get; set; }
    }
}
