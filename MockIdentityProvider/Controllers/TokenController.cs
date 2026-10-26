using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MockIdentityProvider.Core;
using MockIdentityProvider.Core.Tokens.Dtos;
using MockIdentityProvider.Core.Users;

namespace MockIdentityProvider.Controllers
{
    [ApiController]
    [Route("connect")]
    public class TokenController : Controller
    {
        private readonly ITokenService _tokenService;
        private readonly IUserInfo _userInfo;
        public TokenController(ITokenService tokenService, IUserInfo userInfo)
        {
            _tokenService = tokenService;
            _userInfo = userInfo;
        }

        [HttpPost("token")]
        public async Task<ActionResult<TokenResponse>> GenerateTokenAsync([FromForm] TokenRequest request)
        {
            if (request == null)
                return BadRequest(new { error = "invalid_request" });

            if (request.GrantType != "authorization_code" && request.GrantType != "client_credentials" && request.GrantType != "refresh_token")
                return BadRequest(new { error = "unsupported_grant_type" });

            try
            {
                var token = await _tokenService.CreateTokenAsync(request);
                return Ok(token);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = "invalid_grant", error_description = ex.Message });
            }
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout([FromQuery] LogoutRequest request)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!string.IsNullOrEmpty(request.post_logout_redirect_uri))
            {
                var redirectUri = request.post_logout_redirect_uri;
                if (!string.IsNullOrEmpty(request.state))
                    redirectUri += $"?state={Uri.EscapeDataString(request.state)}";

                return Redirect(redirectUri);
            }

            return Ok(new { message = "Logged out" });
        }

        [HttpPost("userinfo")]
        [HttpGet("userinfo")]
        public async Task<ActionResult<UserDto>> GetUserInfo([FromHeader(Name = "Authorization")] string? authorization)
        {
            var accessToken = authorization?.StartsWith("Bearer ") == true
                ? authorization["Bearer ".Length..]
                : null;

            if (string.IsNullOrEmpty(accessToken))
                return BadRequest(new { error = "missing token" });

            var user = await _userInfo.GetUserInfoAsync(accessToken);
            return Ok(user);
        }
    }
}
