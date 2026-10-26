using MockIdentityProvider.Core.Users.Dtos;
using MockIdentityProvider.Domain.Models;

namespace MockIdentityProvider.Core.Users
{
    public interface IUserService
    {
        Task<string> CreateNewUserAsync(UserDto request, string realm);
        Task<bool> UpdateUserAsync(UserDto request, string realm);
        Task<bool> DeleteUserAsync(string userId, string realm);
        Task<UserDto?> GetUserAsync(string userId, string realm);
        Task<PagedResult<UserDto>> SearchUsersAsync(UserSearchCriteria criteria, string realm);
        Task<List<DateTime>> GetLoginDatesAsync(string userId, string realm);
        Task<User> GetAsync(string userId, string realm);
        Task<User> GetUserByNameAsync(string username, string realm);
        Task<bool> SetUserPasswordAsync(string userId, string password, string realm);
        Task<List<User>> GetUsersByRoleAsync(string role, string realm);
        Task<List<string>> GetUserRolesAsync(string userId, string realm);
        Task UpdateUserRolesAsync(string userId, List<string> roleNames, string realm);
        Task RemoveRolesFromUserAsync(string userId, List<string> roleNames, string realm);
        Task AddRolesToUserAsync(string userId, List<string> roleNames, string realm);
        Task<List<object>> GetClientRoleObjectsAsync(string clientId, List<string> roleNames, string realm);
        Task<List<User>> GetUsersAsync(string realm);
    }
}
