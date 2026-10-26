using MockIdentityProvider.Domain.Models;

namespace MockIdentityProvider.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username, string realm);
        Task<User?> GetAsync(string userId, string realm);
        Task<List<User>?> GetUsersByRoleAsync(string role, string realm);
        Task<List<string>?> GetUserRolesAsync(string userId, string realm);
        Task<List<User>> GetUsersAsync(string realm);
        Task<User> CreateAsync(User user, string realm);
        Task<bool> UpdateAsync(User user, string realm);
        Task<bool> DeleteAsync(string userId, string realm);
    }
}
