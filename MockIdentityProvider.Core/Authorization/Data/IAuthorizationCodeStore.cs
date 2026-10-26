using MockIdentityProvider.Domain.Models;

namespace MockIdentityProvider.Core.Authorization.Data
{
    public interface IAuthorizationCodeStore
    {
        public AuthorizationCode? Get(string code);
        public void Save(AuthorizationCode code);
    }
}
