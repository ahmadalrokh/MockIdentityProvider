using System.Text.Json.Serialization;

namespace MockIdentityProvider.Domain.Models
{
    public class User
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IEnumerable<Role> Roles { get; set; } = new List<Role>();
        public Dictionary<string, IEnumerable<string>> Attributes { get; set; } = new();
    }
}
