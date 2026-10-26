using MockIdentityProvider.Domain.Models;

namespace MockIdentityProvider.Domain.TokenGenerator
{
    public interface ITokenGenerator
    {
        Task<string> GenerateIdToken(User user, string clientId, string sessionId, string? nonce = null);
        Task<string> GenerateAccessToken(User user, string clientId, string sessionId);
        Task<string> GenerateRefreshToken(User user, string clientId, string sessionId);
        Task<string> GenerateClientCredentialsToken(ServiceClient serviceClient);
    }
}
