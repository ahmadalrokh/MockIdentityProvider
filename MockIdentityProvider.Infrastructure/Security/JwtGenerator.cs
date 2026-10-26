using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MockIdentityProvider.Domain.Models;
using MockIdentityProvider.Domain.TokenGenerator;
using MockIdentityProvider.Infrastructure.Configurations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace MockIdentityProvider.Infrastructure.Security
{
    internal class JwtGenerator : ITokenGenerator
    {
        private readonly JwtSettings _jwtSettings;

        public JwtGenerator(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        public Task<string> GenerateIdToken(User user, string clientId, string sessionId, string? nonce = null)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, now.ToString(), ClaimValueTypes.Integer64),
                new(JwtRegisteredClaimNames.Iss, _jwtSettings.Issuer),
                new(JwtRegisteredClaimNames.Aud, clientId),
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Sid, sessionId),
                new(JwtRegisteredClaimNames.Azp, clientId),
                new(JwtRegisteredClaimNames.Acr, "1"),
                new("auth_time", now.ToString(), ClaimValueTypes.Integer64),
                new(JwtRegisteredClaimNames.PreferredUsername, user.Username),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.EmailVerified, "true", ClaimValueTypes.Boolean),
                new(JwtRegisteredClaimNames.GivenName, user.FirstName ?? string.Empty),
                new(JwtRegisteredClaimNames.FamilyName, user.LastName ?? string.Empty),
                new(JwtRegisteredClaimNames.Name, $"{user.FirstName} {user.LastName}")
            };

            if (!string.IsNullOrEmpty(nonce))
                claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, nonce));

            return Task.FromResult(BuildToken(claims, _jwtSettings.IdTokenExpiryMinutes));
        }

        public Task<string> GenerateAccessToken(User user, string clientId, string sessionId)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, $"onrtac:{Guid.NewGuid()}"),
                new(JwtRegisteredClaimNames.Iat, now.ToString(), ClaimValueTypes.Integer64),
                new(JwtRegisteredClaimNames.Iss, _jwtSettings.Issuer),
                new(JwtRegisteredClaimNames.Aud, clientId),
                new(JwtRegisteredClaimNames.Azp, clientId),
                new(JwtRegisteredClaimNames.Sid, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Acr, "1"),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.EmailVerified, "true", ClaimValueTypes.Boolean),
                new(JwtRegisteredClaimNames.Name,$"{user.FirstName} {user.LastName}"),
                new(JwtRegisteredClaimNames.PreferredUsername,user.Username),
                new(JwtRegisteredClaimNames.GivenName,user.FirstName ?? string.Empty),
                new(JwtRegisteredClaimNames.FamilyName,user.LastName ?? string.Empty),
                new("scope", "openid profile email")
            };

            var rolesByClient = user.Roles
                .GroupBy(r => r.ClientId)
                .ToDictionary(
                    x => x.Key,
                    x => x.Select(r => r.Name).ToArray()
                );

            var resourceAccess = rolesByClient
                .ToDictionary(
                    x => x.Key,
                    x => new { roles = x.Value }
                );

            claims.Add(new Claim("resource_access", JsonSerializer.Serialize(resourceAccess), JsonClaimValueTypes.Json));

            return Task.FromResult(BuildToken(claims, _jwtSettings.AccessTokenExpiryMinutes));
        }

        public Task<string> GenerateRefreshToken(User user, string clientId, string sessionId)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, $"onrtref:{Guid.NewGuid()}"),
                new(JwtRegisteredClaimNames.Iat, now.ToString(), ClaimValueTypes.Integer64),
                new(JwtRegisteredClaimNames.Iss, _jwtSettings.Issuer),
                new(JwtRegisteredClaimNames.Aud, clientId),
                new("azp", clientId),
                new(JwtRegisteredClaimNames.Sid, sessionId),
                new("typ", "Refresh")
            };

            return Task.FromResult(BuildToken(claims, 30));
        }
        public Task<string> GenerateClientCredentialsToken(ServiceClient serviceClient)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, $"trrtcc:{Guid.NewGuid()}"),
                new(JwtRegisteredClaimNames.Iat, now.ToString(), ClaimValueTypes.Integer64),
                new(JwtRegisteredClaimNames.Iss, _jwtSettings.Issuer),
                new(JwtRegisteredClaimNames.Sub, serviceClient.Sub ?? Guid.NewGuid().ToString()),
                new("typ", "Bearer"),
                new(JwtRegisteredClaimNames.Azp, serviceClient.ClientId),
                new(JwtRegisteredClaimNames.Acr, "1"),
                new("scope", string.Join(" ", serviceClient.Scopes)),
                new("clientHost", "127.0.0.1"),
                new(JwtRegisteredClaimNames.EmailVerified, "false", ClaimValueTypes.Boolean),
                new("LastSyncedUserDate", DateTime.UtcNow.ToString("O")),
                new(JwtRegisteredClaimNames.PreferredUsername, serviceClient.PreferredUsername),
                new("clientAddress", "127.0.0.1"),
                new("client_id", serviceClient.ClientId),
            };

            foreach (var aud in serviceClient.Audiences)
                claims.Add(new Claim(JwtRegisteredClaimNames.Aud, aud));

            var originsJson = JsonSerializer.Serialize(serviceClient.AllowedOrigins);
            claims.Add(new Claim("allowed-origins", originsJson, JsonClaimValueTypes.Json));

            var realmAccessJson = JsonSerializer.Serialize(new { roles = serviceClient.RealmAccess.Roles });
            claims.Add(new Claim("realm_access", realmAccessJson, JsonClaimValueTypes.Json));

            var resourceAccess = serviceClient.ResourceAccess
                .ToDictionary(x => x.Key, x => new { roles = x.Value.Roles });
            claims.Add(new Claim("resource_access", JsonSerializer.Serialize(resourceAccess), JsonClaimValueTypes.Json));

            return Task.FromResult(BuildToken(claims, _jwtSettings.AccessTokenExpiryMinutes));
        }

        private string BuildToken(IEnumerable<Claim> claims, int expiryMinutes)
        {
            var key = new RsaSecurityKey(RsaKeyProvider.Instance)
            {
                KeyId = RsaKeyProvider.KeyId
            };
            var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}