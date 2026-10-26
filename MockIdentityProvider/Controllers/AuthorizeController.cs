using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MockIdentityProvider.Core.Authorization.Data;
using MockIdentityProvider.Core.Authorization.Dto;
using MockIdentityProvider.Domain.Models;
using MockIdentityProvider.ViewModel;
using MockIdentityProvider.ViewModelBuilders;
using System.Security.Claims;

namespace MockIdentityProvider.Controllers
{
    [Route("connect/authorize")]
    public class AuthorizeController : Controller
    {
        private readonly Core.Authorization.IAuthenticationService _authService;
        private readonly IAuthorizationRequestCache _requestCache;
        private readonly LoginViewModelBuilder _viewModelBuilder;
        public AuthorizeController(
           Core.Authorization.IAuthenticationService authService,
            LoginViewModelBuilder viewModelBuilder,
            IAuthorizationRequestCache requestCache)
        {
            _authService = authService;
            _viewModelBuilder = viewModelBuilder;
            _requestCache = requestCache;
        }

        [HttpGet]
        public async Task<IActionResult> Authorize([FromQuery] AuthorizeRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.ClientId))
                return BadRequest("Invalid authorize request");

            var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (authenticateResult.Succeeded && authenticateResult.Principal?.Identity?.IsAuthenticated == true)
            {
                var userId = authenticateResult.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    var authResult = await _authService.HandleAuthorizeAsync(new AuthorizeContext
                    {
                        SelectedUserId = userId,
                        AuthorizeRequest = request
                    });

                    if (authResult.Success && !string.IsNullOrEmpty(authResult.RedirectUrl))
                    {
                        return Redirect(authResult.RedirectUrl);
                    }
                }
            }

            var authzId = Guid.NewGuid().ToString("N");
            await _requestCache.SaveAsync(authzId, request, TimeSpan.FromMinutes(10));

            var model = await _viewModelBuilder.BuildLoginViewModel(request);
            model.AuthzId = authzId;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Authorize(LoginViewModel model)
        {
            var request = await _requestCache.GetAsync(model.AuthzId);
            if (request == null)
                return BadRequest("Authorization request expired or not found. Please restart the login flow.");

            var authResult = await _authService.HandleAuthorizeAsync(new AuthorizeContext
            {
                SelectedUserId = model.SelectedUserId,
                AuthorizeRequest = request
            });

            if (!authResult.Success)
            {
                ModelState.AddModelError(string.Empty, authResult.ErrorMessage ?? "Login failed");

                var newModel = await _viewModelBuilder.BuildLoginViewModel(request);
                newModel.AuthzId = model.AuthzId;
                newModel.SelectedUserId = model.SelectedUserId;

                return View(newModel);
            }

            SetAuthCookie(model.SelectedUserId);

            return Redirect(authResult.RedirectUrl ?? throw new InvalidOperationException(nameof(authResult.RedirectUrl)));
        }

        private void SetAuthCookie(string userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity)).Wait();
        }


    }
}
