using MockIdentityProvider.Core.Tokens.Dtos;

namespace MockIdentityProvider.Core
{
    public interface ITokenService
    {
        Task<TokenResponse> CreateTokenAsync(TokenRequest request);
    }
}
