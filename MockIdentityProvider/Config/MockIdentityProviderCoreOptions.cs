namespace MockIdentityProvider.Config
{
    public class MockIdentityProviderCoreOptions
    {
        public const string CONFIG_KEY = "MockIdentityProviderCore";
        public string[] AllowedOrigins { get; set; }
        public string[] AllowedMethods { get; set; }
    }
}
