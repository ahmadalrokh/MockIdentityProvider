using MockIdentityProvider.Core.Clients.Dtos;
using MockIdentityProvider.Domain.Models;
using MockIdentityProvider.Domain.Repositories;

namespace MockIdentityProvider.Core.Clients
{
    internal class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepository;

        public ClientService(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        public async Task<Client> GetAsync(ClientContext context)
        {
            if (string.IsNullOrEmpty(context.ClientId))
                throw new NullReferenceException(nameof(context.ClientId));

            var client = await _clientRepository.GetClientAsync(context.ClientId);
            if (client == null)
                throw new UnauthorizedAccessException($"Unauthorized Client : {context.ClientId}");

            if (!IsValidRedirectUrl(client, context.RedirectUrl))
                throw new InvalidOperationException($"Invalid redirect url: {context.RedirectUrl} for client: {client.ClientId}");

            return client;
        }

        private bool IsValidRedirectUrl(Client client, string redirectUrl)
        {
            var isValidRedirect = client.RedirectUrls.Any(allowed =>
            {
                // Exact match
                if (allowed.Equals(redirectUrl, StringComparison.OrdinalIgnoreCase))
                    return true;

                if (allowed.EndsWith("/*", StringComparison.Ordinal))
                {
                    var prefix = allowed[..^1];
                    return redirectUrl.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
                }

                return false;
            });

            return isValidRedirect;

        }
    }
}
