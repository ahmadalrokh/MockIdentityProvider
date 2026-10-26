using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MockIdentityProvider.Core;
using MockIdentityProvider.Core.Authorization.Data;
using MockIdentityProvider.Core.Users;
using MockIdentityProvider.Domain.Repositories;
using MockIdentityProvider.Domain.TokenGenerator;
using MockIdentityProvider.Infrastructure.Configurations;
using MockIdentityProvider.Infrastructure.Data.Authorization;
using MockIdentityProvider.Infrastructure.Repositories;
using MockIdentityProvider.Infrastructure.Security;

namespace MockIdentityProvider.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddIdpInfrastructureExtension(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IClientRepository, JsonClientRepository>();

            services.AddScoped<IUserRepository, JsonUserRepository>();

            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
            services.Configure<IdpSettings>(configuration.GetSection(JwtSettings.SectionName));

            services.AddScoped<ITokenGenerator, JwtGenerator>();

            services.AddScoped<IUserInfo, UserInfo>();

            services.AddSingleton<IAuthorizationCodeStore, AuthorizationCodeStore>();

            services.AddScoped<IAuthorizationRequestCache, DistributedAuthorizeRequestCache>();

            return services;
        }
    }
}
