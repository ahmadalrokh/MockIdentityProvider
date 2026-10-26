using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using MockIdentityProvider.Config;
using MockIdentityProvider.Core;
using MockIdentityProvider.Infrastructure;
using MockIdentityProvider.Infrastructure.Configurations;
using MockIdentityProvider.ViewModelBuilders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<LoginViewModelBuilder>();

builder.Services.AddIdpCoreExtension();

builder.Services.AddIdpInfrastructureExtension(builder.Configuration);

builder.Services.AddDistributedMemoryCache();

var jwtSettings = builder.Configuration.GetSection($"{JwtSettings.SectionName}")
                                .Get<JwtSettings>() ?? throw new KeyNotFoundException(nameof(JwtSettings));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.Name = "MockIdentityProvider.Session";
    options.LoginPath = "/connect/authorize";
    options.LogoutPath = "/connect/logout";
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.Authority = jwtSettings.Issuer;
    options.Audience = jwtSettings.Audience;
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = false,
        ValidAudience = jwtSettings.Audience,
        RoleClaimType = "roles",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
    };
    options.RequireHttpsMetadata = false;
});


var corsOriginsConfig = builder.Configuration.GetSection($"{MockIdentityProviderCoreOptions.CONFIG_KEY}")
                                .Get<MockIdentityProviderCoreOptions>() ?? throw new KeyNotFoundException(nameof(MockIdentityProviderCoreOptions.CONFIG_KEY));

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder
        .WithOrigins(corsOriginsConfig.AllowedOrigins)
        .WithMethods(corsOriginsConfig.AllowedMethods)
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/connect/authorize"));

app.Run();
