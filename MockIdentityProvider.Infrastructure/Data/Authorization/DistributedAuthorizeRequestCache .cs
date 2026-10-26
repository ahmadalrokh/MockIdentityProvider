using Microsoft.Extensions.Caching.Distributed;
using MockIdentityProvider.Core.Authorization.Data;
using MockIdentityProvider.Domain.Models;
using System.Text.Json;

namespace MockIdentityProvider.Infrastructure.Data.Authorization
{
    public class DistributedAuthorizeRequestCache : IAuthorizationRequestCache
    {
        private readonly IDistributedCache _distributedCache;

        public DistributedAuthorizeRequestCache(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        private static string Key(string id) => $"authz:{id}";

        public async Task<AuthorizeRequest?> GetAsync(string authzId)
        {
            var json = await _distributedCache.GetStringAsync(Key(authzId));
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JsonSerializer.Deserialize<AuthorizeRequest>(json);
        }

        public Task SaveAsync(string authzId, AuthorizeRequest request, TimeSpan ttl)
        {
            _distributedCache.SetStringAsync(Key(authzId), JsonSerializer.Serialize(request),
                 new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl });

            return Task.CompletedTask;
        }

        public Task RemoveAsync(string authzId)
            => _distributedCache.RemoveAsync(Key(authzId));
    }
}
