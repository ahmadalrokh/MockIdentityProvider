namespace MockIdentityProvider.Domain.Models
{
    public class Client
    {
        public required string ClientId { get; set; }
        public required string Realm { get; set; }
        public List<string> RedirectUrls { get; set; } = new List<string>();
        public required string RolesClient { get; set; }
        public required string UserFile { get; set; }
        public required string SystemName { get; set; }
        public string? ServiceClientFile { get; set; }
    }
}
