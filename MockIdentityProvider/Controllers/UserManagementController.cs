using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MockIdentityProvider.Core.Users;
using MockIdentityProvider.Core.Users.Dtos;

namespace MockIdentityProvider.Controllers
{
    [ApiController]
    [Route("admin/realms/{realm}/users")]
    [Authorize]
    public class UserManagementController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserManagementController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromRoute] string realm, [FromBody] UserDto request)
        {
            var userId = await _userService.CreateNewUserAsync(request, realm);
            return Created($"/admin/realms/{realm}/users/{userId}", null);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser([FromRoute] string realm, [FromRoute] string id, [FromBody] UserDto request)
        {
            request.Id = id;
            var success = await _userService.UpdateUserAsync(request, realm);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] string realm, [FromRoute] string id)
        {
            var success = await _userService.DeleteUserAsync(id, realm);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser([FromRoute] string realm, [FromRoute] string id)
        {
            var user = await _userService.GetUserAsync(id, realm);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpGet]
        public async Task<IActionResult> SearchUsers([FromRoute] string realm, [FromQuery] string? search, [FromQuery] string? username, [FromQuery] string? email, [FromQuery] int first = 0, [FromQuery] int max = 100)
        {
            var criteria = new UserSearchCriteria
            {
                Search = search,
                Username = username,
                Email = email,
                Offset = first,
                Limit = max
            };
            var result = await _userService.SearchUsersAsync(criteria, realm);
            return Ok(result.Data);
        }

        [HttpGet("/admin/realms/{realm}/events")]
        public async Task<IActionResult> GetEvents([FromRoute] string realm, [FromQuery] string type, [FromQuery] string user)
        {
            // MockIdentityProvider doesn't track login events, return empty array to satisfy KeycloakClient
            return Ok(new List<object>());
        }

        [HttpPut("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword([FromRoute] string realm, [FromRoute] string id, [FromBody] object passwordDto)
        {
            await _userService.SetUserPasswordAsync(id, "dummy", realm);
            return NoContent();
        }

        [HttpGet("{id}/role-mappings/clients/{clientUuid}")]
        public async Task<IActionResult> GetClientRoles([FromRoute] string realm, [FromRoute] string id, [FromRoute] string clientUuid)
        {
            var roles = await _userService.GetUserRolesAsync(id, realm);
            var roleObjects = new List<object>();
            foreach (var r in roles)
            {
                roleObjects.Add(new { Id = Guid.NewGuid().ToString(), Name = r, Description = r, ContainerId = clientUuid });
            }
            return Ok(roleObjects);
        }

        [HttpPost("{id}/role-mappings/clients/{clientUuid}")]
        public async Task<IActionResult> AddClientRoles([FromRoute] string realm, [FromRoute] string id, [FromRoute] string clientUuid, [FromBody] List<RoleDto> roles)
        {
            var roleNames = roles.Select(r => r.Name).ToList();
            await _userService.AddRolesToUserAsync(id, roleNames, realm);
            return NoContent();
        }

        [HttpDelete("{id}/role-mappings/clients/{clientUuid}")]
        public async Task<IActionResult> DeleteClientRoles([FromRoute] string realm, [FromRoute] string id, [FromRoute] string clientUuid, [FromBody] List<RoleDto> roles)
        {
            var roleNames = roles.Select(r => r.Name).ToList();
            await _userService.RemoveRolesFromUserAsync(id, roleNames, realm);
            return NoContent();
        }

        [HttpGet("/admin/realms/{realm}/clients/{clientId}/roles/{roleName}")]
        public async Task<IActionResult> GetClientRole([FromRoute] string realm, [FromRoute] string clientId, [FromRoute] string roleName)
        {
            var result = await _userService.GetClientRoleObjectsAsync(clientId, new List<string> { roleName }, realm);
            if (result != null && result.Count > 0)
                return Ok(result[0]);
            return NotFound();
        }

        [HttpGet("/admin/realms/{realm}/clients/{clientId}/roles/{roleName}/users")]
        public async Task<IActionResult> GetUsersByClientRole([FromRoute] string realm, [FromRoute] string clientId, [FromRoute] string roleName, [FromQuery] int first = 0, [FromQuery] int max = 100)
        {
            var users = await _userService.GetUsersByRoleAsync(roleName, realm);

            var pagedUsers = users
                .Skip(first)
                .Take(max)
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

            return Ok(pagedUsers);
        }
    }

    public class RoleDto
    {
        public string? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
