using MockIdentityProvider.Domain.Models;

namespace MockIdentityProvider.Core.Authorization.Dto
{
    public class AuthorizeContext
    {
        public required string SelectedUserId { get; set; }
        public AuthorizeRequest AuthorizeRequest { get; set; } = new();
    }
}
