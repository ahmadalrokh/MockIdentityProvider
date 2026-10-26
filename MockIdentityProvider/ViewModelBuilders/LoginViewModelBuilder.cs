using MockIdentityProvider.Core.Clients;
using MockIdentityProvider.Core.Users;
using MockIdentityProvider.Domain.Models;
using MockIdentityProvider.ViewModel;

namespace MockIdentityProvider.ViewModelBuilders
{
    public class LoginViewModelBuilder
    {
        private readonly IUserService _userService;
        private readonly IClientService _clientService;

        public LoginViewModelBuilder(IUserService userService, IClientService clientService)
        {
            _userService = userService;
            _clientService = clientService;
        }

        public async Task<LoginViewModel> BuildLoginViewModel(AuthorizeRequest request)
        {
            var client = await _clientService.GetAsync(new()
            {
                ClientId = request.ClientId,
                RedirectUrl = request.RedirectUri
            });

            var users = await _userService.GetUsersAsync(client.Realm);

            var model = new LoginViewModel
            {
                SystemName = client.SystemName,
                UserList = users.Select(u => new UserList
                {
                    UserId = u.Id,
                    Username = u.Username,
                    fullName = $"{u.FirstName} {u.LastName}",
                    Department = u.Attributes.GetValueOrDefault("Department")?.FirstOrDefault() ?? string.Empty
                }).OrderBy(u => u.Username).ToList()
            };

            return model;
        }
    }
}
