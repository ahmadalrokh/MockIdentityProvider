using MockIdentityProvider.Core.Authorization.Data;
using MockIdentityProvider.Domain.Models;
using System.Collections.Concurrent;

namespace MockIdentityProvider.Infrastructure.Data.Authorization
{
    public class AuthorizationCodeStore : IAuthorizationCodeStore
    {
        private static readonly ConcurrentDictionary<string, AuthorizationCode> _codes = new();

        public void Save(AuthorizationCode code)
        {
            var expireCodes = _codes.Where(kv => kv.Value.Used || kv.Value.ExpiresAt < DateTime.UtcNow)
                .Select(kv => kv.Key)
                .ToList();

            foreach (var key in expireCodes)
                _codes.TryRemove(key, out _);

            _codes[code.Code] = code;
        }

        public AuthorizationCode? Get(string code)
        {
            _codes.TryGetValue(code, out var authCode);
            return authCode;
        }
    }
}
