using MockIdentityProvider.Domain.Models;

namespace MockIdentityProvider.Core.Authorization.Data
{
    public interface IAuthorizationRequestCache
    {
        Task SaveAsync(string authzId, AuthorizeRequest request, TimeSpan ttl);
        Task<AuthorizeRequest?> GetAsync(string authzId);
        Task RemoveAsync(string authzId);
    }
}
