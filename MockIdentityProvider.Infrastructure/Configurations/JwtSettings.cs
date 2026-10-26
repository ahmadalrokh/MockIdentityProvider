namespace MockIdentityProvider.Infrastructure.Configurations
{
    public class JwtSettings
    {
        public const string SectionName = "jwt";
        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = "MockIdentityProvider-Api";
        public int AccessTokenExpiryMinutes { get; set; } = 15;
        public int IdTokenExpiryMinutes { get; set; } = 15;
    }
}
