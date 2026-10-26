using Microsoft.Extensions.Options;
using MockIdentityProvider.Core.Authorization.Data;
using MockIdentityProvider.Core.Authorization.Dto;
using MockIdentityProvider.Core.Clients;
using MockIdentityProvider.Core.Clients.Dtos;
using MockIdentityProvider.Core.Helpers;
using MockIdentityProvider.Core.Users;
using MockIdentityProvider.Domain.Models;

namespace MockIdentityProvider.Core.Authorization
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IClientService _clientService;
        private readonly IUserService _userService;
        private readonly IAuthorizationCodeStore _codeStore;
        private readonly string _issuer;

        public AuthenticationService(
            IClientService clientService,
            IUserService userService,
            IAuthorizationCodeStore codeStore,
            IOptions<IdpSettings> idpSettings)
        {
            _clientService = clientService;
            _userService = userService;
            _codeStore = codeStore;
            _issuer = idpSettings.Value.Issuer;
        }

        public async Task<AuthResult> HandleAuthorizeAsync(AuthorizeContext context)
        {
            var client = await _clientService.GetAsync(new ClientContext
            {
                ClientId = context.AuthorizeRequest.ClientId,
                RedirectUrl = context.AuthorizeRequest.RedirectUri
            });

            var authentication = await AuthenticateAsync(context.SelectedUserId, client.Realm);
            if (!authentication.Success)
                return authentication;

            var authCode = await GenerateAuthorizationCodeAsync(context, client.Realm);

            var redirectUri = context.AuthorizeRequest.RedirectUri;
            var redirectUrl = $"{redirectUri}" +
                $"#state={Uri.EscapeDataString(context.AuthorizeRequest.State)}" +
                $"&session_state={Uri.EscapeDataString(Guid.NewGuid().ToString())}" +
                $"&iss={Uri.EscapeDataString(_issuer)}" +
                $"&code={Uri.EscapeDataString(authCode.Code)}";

            return ReturnAuthResult(true, null, redirectUrl);
        }

        private async Task<AuthResult> AuthenticateAsync(string userId, string realm)
        {
            if (string.IsNullOrEmpty(userId))
                return ReturnAuthResult(false, "UserId required");

            var user = await _userService.GetUserAsync(userId, realm);

            if (user == null)
                return ReturnAuthResult(false, "User not found");

            return ReturnAuthResult(true);
        }

        private async Task<AuthorizationCode> GenerateAuthorizationCodeAsync(AuthorizeContext context, string realm)
        {
            var code = AuthorizationCodeGenerator.Generate();
            if (code == null)
                throw new InvalidOperationException(nameof(code));

            var authCode = new AuthorizationCode
            {
                Code = code,
                UserId = context.SelectedUserId,
                Realm = realm,
                ClientId = context.AuthorizeRequest.ClientId,
                RedirectUri = context.AuthorizeRequest.RedirectUri,
                Nonce = context.AuthorizeRequest.Nonce,
                CodeChallenge = context.AuthorizeRequest.CodeChallenge,
                CodeChallengeMethod = context.AuthorizeRequest.CodeChallengeMethod,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                Used = false
            };

            _codeStore.Save(authCode);
            return authCode;
        }

        private AuthResult ReturnAuthResult(bool success, string? errorMessage = null, string? redirectUrl = null)
        {
            return new AuthResult
            {
                Success = success,
                ErrorMessage = errorMessage,
                RedirectUrl = redirectUrl
            };
        }
    }
}
