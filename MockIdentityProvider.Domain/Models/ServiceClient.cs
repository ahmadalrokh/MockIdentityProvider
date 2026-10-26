using System.Text.Json.Serialization;

namespace MockIdentityProvider.Domain.Models
{
    public class ServiceClient
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string Realm { get; set; } = string.Empty;
        
        [JsonPropertyName("preferred_username")]
        public string PreferredUsername { get; set; } = string.Empty;
        public string? Sub { get; set; }
        public List<string> Audiences { get; set; } = new();
        public List<string> AllowedOrigins { get; set; } = new();
        public List<string> Scopes { get; set; } = new() { "profile", "email" };
        public RealmAccess RealmAccess { get; set; } = new();
        public Dictionary<string, RoleAccess> ResourceAccess { get; set; } = new();
    }

    public class RealmAccess
    {
        public List<string> Roles { get; set; } = new();
    }

    public class RoleAccess
    {
        public List<string> Roles { get; set; } = new();
    }
}
