namespace MockIdentityProvider.Core
{
    public class IdpSettings
    {
        public string Issuer { get; set; } = string.Empty;
        public int AccessTokenExpiryMinutes { get; set; } = 15;
    }
}
