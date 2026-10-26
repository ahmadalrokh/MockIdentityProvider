using MockIdentityProvider.Core.Users;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace MockIdentityProvider.Infrastructure.Security
{
    internal class UserInfo : IUserInfo
    {
        public async Task<UserDto> GetUserInfoAsync(string accessToken)
        {
            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(accessToken))
                throw new ArgumentException("Invalid token format");

            var token = handler.ReadJwtToken(accessToken);

            var claims = token.Claims;

            var user = new UserDto
            {
                Id = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value ?? string.Empty,
                Email = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value ?? string.Empty,
                Username = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.PreferredUsername)?.Value ?? string.Empty,
                FirstName = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.GivenName)?.Value ?? string.Empty,
                LastName = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.FamilyName)?.Value ?? string.Empty,
                Roles = GetRolesFromToken(token)
            };

            return await Task.FromResult(user);
        }

        private List<string> GetRolesFromToken(JwtSecurityToken token)
        {
            var resourceAccessJson = token.Claims
                .FirstOrDefault(c => c.Type == "resource_access")?.Value;

            if (string.IsNullOrEmpty(resourceAccessJson))
                return new List<string>();

            using var doc = JsonDocument.Parse(resourceAccessJson);

            var roles = new List<string>();

            foreach (var client in doc.RootElement.EnumerateObject())
            {
                if (client.Value.TryGetProperty("roles", out var rolesElement))
                {
                    foreach (var role in rolesElement.EnumerateArray())
                    {
                        roles.Add(role.GetString() ?? string.Empty);
                    }
                }
            }

            return roles.Distinct().ToList();
        }

    }
}
