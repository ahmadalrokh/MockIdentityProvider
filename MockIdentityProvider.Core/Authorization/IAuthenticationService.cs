using MockIdentityProvider.Core.Authorization.Dto;
using MockIdentityProvider.Domain.Models;

namespace MockIdentityProvider.Core.Authorization
{
    public interface IAuthenticationService
    {
        Task<AuthResult> HandleAuthorizeAsync(AuthorizeContext context);
    }
}
