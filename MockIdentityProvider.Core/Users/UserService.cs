using MockIdentityProvider.Core.Users.Dtos;
using MockIdentityProvider.Domain.Models;
using MockIdentityProvider.Domain.Repositories;
using System.ComponentModel.DataAnnotations;

namespace MockIdentityProvider.Core.Users
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IClientRepository _clientRepository;
        public UserService(IUserRepository userRepository, IClientRepository clientRepository)
        {
            _userRepository = userRepository;
            _clientRepository = clientRepository;
        }

        public async Task<string> CreateNewUserAsync(UserDto request, string realm)
        {
            var client = await _clientRepository.GetClientByRealmAsync(realm);
            if (client == null)
                throw new UnauthorizedAccessException($"No client found for this realm: {realm}");

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = request.Username,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Attributes = request.Attributes,
                Roles = request.Roles == null ? new List<Role>
                {
                    new Role{Name = "Defulte Role" , ClientId = client.RolesClient},
                } :
                request.Roles.Select(r => new Role { Name = r, ClientId = client.RolesClient }).ToList()
            };

            var createdUser = await _userRepository.CreateAsync(user, realm);
            return createdUser.Id;
        }

        public async Task<bool> UpdateUserAsync(UserDto request, string realm)
        {
            var user = await _userRepository.GetAsync(request.Id, realm);
            if (user == null)
                return false;

            user.Username = request.Username;
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.Attributes = request.Attributes;

            return await _userRepository.UpdateAsync(user, realm);
        }

        public async Task<bool> DeleteUserAsync(string userId, string realm)
        {
            return await _userRepository.DeleteAsync(userId, realm);
        }

        public async Task<UserDto?> GetUserAsync(string userId, string realm)
        {
            var user = await _userRepository.GetAsync(userId, realm);
            if (user == null)
                return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Roles = user.Roles?.Select(r => r.Name).ToList() ?? new List<string>(),
                Attributes = user.Attributes
            };
        }

        public async Task<PagedResult<UserDto>> SearchUsersAsync(UserSearchCriteria criteria, string realm)
        {
            var users = await _userRepository.GetUsersAsync(realm);

            var query = users.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(criteria.Search))
            {
                var lowerSearch = criteria.Search.ToLower();
                query = query.Where(u => u.Username.ToLower().Contains(lowerSearch) ||
                                         u.Email.ToLower().Contains(lowerSearch) ||
                                         u.FirstName.ToLower().Contains(lowerSearch) ||
                                         u.LastName.ToLower().Contains(lowerSearch));
            }

            if (!string.IsNullOrWhiteSpace(criteria.Username))
            {
                query = query.Where(u => u.Username.Equals(criteria.Username, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(criteria.Email))
            {
                query = query.Where(u => u.Email.Equals(criteria.Email, StringComparison.OrdinalIgnoreCase));
            }

            var totalCount = query.Count();

            var pagedUsers = query
                .Skip(criteria.Offset)
                .Take(criteria.Limit)
                .Select(user => new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Roles = user.Roles?.Select(r => r.Name).ToList() ?? new List<string>(),
                    Attributes = user.Attributes
                })
                .ToList();

            return new PagedResult<UserDto>
            {
                Data = pagedUsers,
                TotalCount = totalCount
            };
        }

        public Task<List<DateTime>> GetLoginDatesAsync(string userId, string realm)
        {
            // Dummy implementation as login dates are not tracked
            return Task.FromResult(new List<DateTime>());
        }

        public Task<bool> SetUserPasswordAsync(string userId, string password, string realm)
        {
            // MockIdentityProvider doesn't have password, just return true
            return Task.FromResult(true);
        }

        public async Task<User> GetAsync(string userId, string realm)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(realm))
                throw new NullReferenceException();

            var user = await _userRepository.GetAsync(userId, realm);
            if (user == null)
                throw new ValidationException($"user with id:{userId} not found");

            return user;
        }

        public async Task<User> GetUserByNameAsync(string username, string realm)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(realm))
                throw new NullReferenceException();

            var user = await _userRepository.GetByUsernameAsync(username, realm);
            if (user == null)
                throw new ValidationException($"user with name:{username} not found");

            return user;
        }

        public async Task<List<User>> GetUsersByRoleAsync(string role, string realm)
        {
            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(realm))
                throw new NullReferenceException();

            var user = await _userRepository.GetUsersByRoleAsync(role, realm);
            if (user == null)
                throw new ValidationException($"users with role: {role} not found");

            return user;
        }

        public async Task<List<string>> GetUserRolesAsync(string userId, string realm)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(realm))
                throw new NullReferenceException();

            var roles = await _userRepository.GetUserRolesAsync(userId, realm);

            return roles;
        }

        public Task<List<User>> GetUsersAsync(string realm)
        {
            return _userRepository.GetUsersAsync(realm);
        }

        public async Task UpdateUserRolesAsync(string userId, List<string> roleNames, string realm)
        {
            var user = await _userRepository.GetAsync(userId, realm);
            if (user != null)
            {
                user.Roles = roleNames.Select(r => new Role { Name = r, ClientId = "web-api-client" }).ToList();
                await _userRepository.UpdateAsync(user, realm);
            }
        }

        public async Task RemoveRolesFromUserAsync(string userId, List<string> roleNames, string realm)
        {
            var user = await _userRepository.GetAsync(userId, realm);
            if (user != null && user.Roles != null)
            {
                var rolesList = user.Roles.ToList();
                rolesList.RemoveAll(r => roleNames.Contains(r.Name));
                user.Roles = rolesList;
                await _userRepository.UpdateAsync(user, realm);
            }
        }

        public async Task AddRolesToUserAsync(string userId, List<string> roleNames, string realm)
        {
            var user = await _userRepository.GetAsync(userId, realm);
            if (user != null)
            {
                var rolesList = user.Roles?.ToList() ?? new List<Role>();
                var existingRoleNames = rolesList.Select(r => r.Name).ToHashSet();

                foreach (var roleName in roleNames)
                {
                    if (!existingRoleNames.Contains(roleName))
                    {
                        rolesList.Add(new Role { Name = roleName, ClientId = "web-api-client" });
                    }
                }

                user.Roles = rolesList;
                await _userRepository.UpdateAsync(user, realm);
            }
        }

        public Task<List<object>> GetClientRoleObjectsAsync(string clientId, List<string> roleNames, string realm)
        {
            var roles = roleNames.Select(r => (object)new { Id = Guid.NewGuid().ToString(), Name = r, Description = r }).ToList();
            return Task.FromResult(roles);
        }
    }
}
