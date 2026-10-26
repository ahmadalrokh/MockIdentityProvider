using Microsoft.Extensions.DependencyInjection;
using MockIdentityProvider.Core.Authorization;
using MockIdentityProvider.Core.Clients;
using MockIdentityProvider.Core.Users;

namespace MockIdentityProvider.Core
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddIdpCoreExtension(this IServiceCollection services)
        {
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<IUserService, UserService>();



            services.AddScoped<ITokenService, TokenService>();

            return services;
        }
    }
}
