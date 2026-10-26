using MockIdentityProvider.Domain.Models;

namespace MockIdentityProvider.Domain.Repositories
{
    public interface IClientRepository
    {
        Task<Client?> GetClientAsync(string clientId);
        Task<Client?> GetClientByRealmAsync(string realm);
        Task<ServiceClient?> GetServiceClientAsync(string clientId);
    }
}
