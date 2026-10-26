using MockIdentityProvider.Domain.Models;
using MockIdentityProvider.Domain.Repositories;
using System.Text.Json;

namespace MockIdentityProvider.Infrastructure.Repositories
{
    internal class JsonClientRepository : IClientRepository
    {
        private readonly string _clientFolder;

        public JsonClientRepository()
        {
            _clientFolder = Path.Combine(AppContext.BaseDirectory, "Data", "Clients");
        }

        public async Task<Client?> GetClientAsync(string clientId)
        {
            var clients = await GetClientsAsync();

            return clients?.FirstOrDefault(c => c.ClientId == clientId);
        }

        public async Task<Client?> GetClientByRealmAsync(string realm)
        {
            var clients = await GetClientsAsync();

            return clients?.FirstOrDefault(c => c.Realm.ToLower() == realm.ToLower());
        }

        public async Task<ServiceClient?> GetServiceClientAsync(string clientId)
        {
            var files = Directory.GetFiles(_clientFolder, "*-service-clients.json");
            foreach (var file in files)
            {
                var json = await File.ReadAllTextAsync(file);

                var serviceClients = JsonSerializer.Deserialize<List<ServiceClient>>(json,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                var match = serviceClients?.FirstOrDefault(sc => sc.ClientId == clientId);

                if (match != null) return match;
            }

            return null;
        }

        private async Task<List<Client>?> GetClientsAsync()
        {
            var filePath = Path.Combine(_clientFolder, "clients.json");

            if (!File.Exists(filePath))
                return null;

            var json = await File.ReadAllTextAsync(filePath);

            var clients = JsonSerializer.Deserialize<List<Client>>(json, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            return clients;
        }

    }
}
