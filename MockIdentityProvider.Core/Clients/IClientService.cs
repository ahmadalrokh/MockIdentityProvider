using MockIdentityProvider.Core.Clients.Dtos;
using MockIdentityProvider.Domain.Models;

namespace MockIdentityProvider.Core.Clients
{
    public interface IClientService
    {
        Task<Client> GetAsync(ClientContext context);
    }
}
