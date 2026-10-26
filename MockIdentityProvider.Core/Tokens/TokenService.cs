using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MockIdentityProvider.Core.Authorization.Data;
using MockIdentityProvider.Core.Tokens.Dtos;
using MockIdentityProvider.Core.Users;
using MockIdentityProvider.Domain.Models;
using MockIdentityProvider.Domain.Repositories;
using MockIdentityProvider.Domain.TokenGenerator;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

namespace MockIdentityProvider.Core
{
    public class TokenService : ITokenService
    {
        private readonly IUserService _userService;
        private readonly ITokenGenerator _jwtGenerator;
        private readonly IAuthorizationCodeStore _codeStore;
        private readonly IClientRepository _clientRepository;
        private readonly IdpSettings _idpSettings;

        public TokenService(
            IUserService userService,
            ITokenGenerator jwtGenerator,
            IAuthorizationCodeStore codeStore,
            IOptions<IdpSettings> idpSettings,
            IClientRepository clientRepository)
        {
            _userService = userService;
            _jwtGenerator = jwtGenerator;
            _codeStore = codeStore;
            _idpSettings = idpSettings.Value;
            _clientRepository = clientRepository;
        }

        public async Task<TokenResponse> CreateTokenAsync(TokenRequest request)
        {
            if (request.GrantType == "authorization_code")
            {
                if (string.IsNullOrEmpty(request.Code))
                    throw new NullReferenceException(nameof(request.Code));

                var authCode = await ValidateAuthorizationCodeAsync(request);

                var user = await _userService.GetAsync(authCode.UserId, authCode.Realm);

                return await BuildTokenResponse(user, request, authCode);
            }
            else if (request.GrantType == "client_credentials")
            {
                if (string.IsNullOrEmpty(request.ClientId))
                    throw new NullReferenceException(nameof(request.ClientId));

                if (string.IsNullOrEmpty(request.ClientSecret))
                    throw new NullReferenceException(nameof(request.ClientSecret));


                return await HandleClientCredentialsAsync(request);
            }
            else if (request.GrantType == "refresh_token")
            {
                if (string.IsNullOrEmpty(request.RefreshToken))
                    throw new NullReferenceException(nameof(request.RefreshToken));
                if (string.IsNullOrEmpty(request.ClientId))
                    throw new NullReferenceException(nameof(request.ClientId));

                return await HandleRefreshTokenAsync(request);
            }

            throw new UnauthorizedAccessException("Invalid grant type");
        }



        private async Task<AuthorizationCode> ValidateAuthorizationCodeAsync(TokenRequest request)
        {
            if (string.IsNullOrEmpty(request.Code))
                throw new UnauthorizedAccessException("Authorization code not found.");

            var authCode = _codeStore.Get(request.Code);
            if (authCode == null)
                throw new UnauthorizedAccessException("Authorization code not found.");

            if (string.IsNullOrEmpty(authCode.Realm))
                throw new UnauthorizedAccessException("realm mismatch.");

            if (string.IsNullOrEmpty(authCode.UserId))
                throw new UnauthorizedAccessException("user_id mismatch.");

            if (authCode.ClientId != request.ClientId)
                throw new UnauthorizedAccessException("client_id mismatch.");

            if (authCode.ExpiresAt < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Authorization code expired.");

            if (authCode.Used)
                throw new UnauthorizedAccessException("Authorization code already used.");

            if (!string.IsNullOrEmpty(authCode.RedirectUri) &&
                authCode.RedirectUri != request.RedirectUri)
                throw new UnauthorizedAccessException("redirect_uri mismatch.");

            if (!string.IsNullOrEmpty(authCode.CodeChallenge))
            {
                if (string.IsNullOrEmpty(request.CodeVerifier))
                    throw new UnauthorizedAccessException("code_verifier is required (PKCE).");

                var computed = ComputeS256Challenge(request.CodeVerifier);
                if (computed != authCode.CodeChallenge)
                    throw new UnauthorizedAccessException("PKCE verification failed.");
            }

            authCode.Used = true;
            return authCode;
        }

        private async Task<TokenResponse> HandleClientCredentialsAsync(TokenRequest request)
        {
            var serviceClient = await _clientRepository.GetServiceClientAsync(request.ClientId);

            if (serviceClient == null)
                throw new UnauthorizedAccessException($"Service client '{request.ClientId}' not found.");

            if (serviceClient.ClientSecret != request.ClientSecret)
                throw new UnauthorizedAccessException("Invalid client secret.");

            var accessToken = await _jwtGenerator.GenerateClientCredentialsToken(serviceClient);

            return new TokenResponse
            {
                AccessToken = accessToken,
                TokenType = "Bearer",
                ExpiresIn = _idpSettings.AccessTokenExpiryMinutes,
            };
        }

        private async Task<TokenResponse> HandleRefreshTokenAsync(TokenRequest request)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                if (!handler.CanReadToken(request.RefreshToken))
                    throw new UnauthorizedAccessException("Invalid refresh token format.");

                var jwtToken = handler.ReadJwtToken(request.RefreshToken);

                if (jwtToken.ValidTo < DateTime.UtcNow)
                    throw new UnauthorizedAccessException("Refresh token expired.");

                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
                var clientId = jwtToken.Claims.FirstOrDefault(c => c.Type == "azp" || c.Type == "aud")?.Value;
                var sessionId = jwtToken.Claims.FirstOrDefault(c => c.Type == "sid" || c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sid)?.Value ?? Guid.NewGuid().ToString();

                if (string.IsNullOrEmpty(userId) || clientId != request.ClientId)
                    throw new UnauthorizedAccessException("Invalid refresh token claims.");

                var client = await _clientRepository.GetClientAsync(clientId);
                if (client == null)
                    throw new UnauthorizedAccessException("Client not found.");

                var user = await _userService.GetAsync(userId, client.Realm);
                if (user == null)
                    throw new UnauthorizedAccessException("User not found.");

                var accessToken = await _jwtGenerator.GenerateAccessToken(user, request.ClientId, sessionId);
                var idToken = await _jwtGenerator.GenerateIdToken(user, request.ClientId, sessionId);
                var newRefreshToken = await _jwtGenerator.GenerateRefreshToken(user, request.ClientId, sessionId);

                return new TokenResponse
                {
                    AccessToken = accessToken,
                    IdToken = idToken,
                    RefreshToken = newRefreshToken,
                    ExpiresIn = _idpSettings.AccessTokenExpiryMinutes,
                    RefreshExpiresIn = 1800,
                    SessionState = sessionId,
                    TokenType = "Bearer",
                };
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException("Invalid refresh token format.", ex);
            }
        }

        private async Task<TokenResponse> BuildTokenResponse(User user, TokenRequest request, AuthorizationCode authCode)
        {
            var sessionId = Guid.NewGuid().ToString();

            var accessToken = await _jwtGenerator.GenerateAccessToken(user, request.ClientId, sessionId);
            var idToken = await _jwtGenerator.GenerateIdToken(user, request.ClientId, sessionId, authCode.Nonce);
            var refreshToken = await _jwtGenerator.GenerateRefreshToken(user, request.ClientId, sessionId);

            return new TokenResponse
            {
                AccessToken = accessToken,
                IdToken = idToken,
                RefreshToken = refreshToken,
                ExpiresIn = _idpSettings.AccessTokenExpiryMinutes,
                RefreshExpiresIn = 1800,
                SessionState = sessionId,
                TokenType = "Bearer",
            };
        }

        private static string ComputeS256Challenge(string codeVerifier)
        {
            var bytes = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
            return Base64UrlEncoder.Encode(bytes);
        }
    }
}
