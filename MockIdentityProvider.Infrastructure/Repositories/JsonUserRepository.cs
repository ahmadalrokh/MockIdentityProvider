using MockIdentityProvider.Domain.Models;
using MockIdentityProvider.Domain.Repositories;
using System.Text.Json;

namespace MockIdentityProvider.Infrastructure.Repositories
{
    internal class JsonUserRepository : IUserRepository
    {
        private readonly string _usersFolder;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public JsonUserRepository()
        {
            _usersFolder = Path.Combine(AppContext.BaseDirectory, "Data", "Users");
        }

        public async Task<User?> GetByUsernameAsync(string username, string realm)
        {
            var users = await GetUsersAsync(realm);
            return users?.FirstOrDefault(x => x.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<User?> GetAsync(string userId, string realm)
        {
            var users = await GetUsersAsync(realm);
            return users.FirstOrDefault(x => x.Id == userId);
        }

        public async Task<List<User>?> GetUsersByRoleAsync(string role, string realm)
        {
            var users = await GetUsersAsync(realm);
            return users.Where(u => u.Roles.Select(r => r.Name).Contains(role)).ToList();
        }

        public async Task<List<string>?> GetUserRolesAsync(string userId, string realm)
        {
            var users = await GetUsersAsync(realm);

            var roles = users.Where(u => u.Id == userId)
                .SelectMany(u => u.Roles.Select(r => r.Name)).ToList();

            return roles;
        }

        public async Task<List<User>> GetUsersAsync(string realm)
        {
            var filePath = Path.Combine(_usersFolder, $"{realm.ToLower()}-users.json");
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Could not find the users file.", filePath);

            var json = await File.ReadAllTextAsync(filePath);

            var users = JsonSerializer.Deserialize<List<User>>(json);
            if (users == null || users.Count == 0)
                throw new InvalidOperationException("No users found for this realm.");

            return users;
        }

        public async Task<User> CreateAsync(User user, string realm)
        {
            await _semaphore.WaitAsync();
            try
            {
                var users = await GetUsersInternalAsync(realm);
                users.Add(user);
                await SaveUsersAsync(users, realm);
                return user;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> UpdateAsync(User user, string realm)
        {
            await _semaphore.WaitAsync();
            try
            {
                var users = await GetUsersInternalAsync(realm);
                var existingUser = users.FirstOrDefault(u => u.Id == user.Id);
                if (existingUser != null)
                {
                    users.Remove(existingUser);
                    users.Add(user);
                    await SaveUsersAsync(users, realm);
                    return true;
                }
                return false;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> DeleteAsync(string userId, string realm)
        {
            await _semaphore.WaitAsync();
            try
            {
                var users = await GetUsersInternalAsync(realm);
                var existingUser = users.FirstOrDefault(u => u.Id == userId);
                if (existingUser != null)
                {
                    users.Remove(existingUser);
                    await SaveUsersAsync(users, realm);
                    return true;
                }
                return false;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<List<User>> GetUsersInternalAsync(string realm)
        {
            var filePath = Path.Combine(_usersFolder, $"{realm.ToLower()}-users.json");
            if (!File.Exists(filePath))
                return new List<User>();

            var json = await File.ReadAllTextAsync(filePath);
            var users = JsonSerializer.Deserialize<List<User>>(json);
            return users ?? new List<User>();
        }

        private async Task SaveUsersAsync(List<User> users, string realm)
        {
            var filePath = Path.Combine(_usersFolder, $"{realm.ToLower()}-users.json");
            var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
        }
    }
}
