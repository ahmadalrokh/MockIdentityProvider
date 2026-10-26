# Mock Identity Provider

A lightweight, Keycloak-compatible **OpenID Connect identity provider for local development and testing**.
Instead of running a full Keycloak instance, front-end apps and APIs can sign in against this mock: you pick a
user from a list (no passwords), and it issues real, RS256-signed JWTs with the roles and attributes your apps expect.

> ⚠️ For development and testing only. There are no passwords, the signing key is regenerated on every start,
> and all data lives in JSON files.

## Features

- **OIDC discovery** – `/.well-known/openid-configuration` and `/.well-known/jwks.json`
- **Authorization Code flow** with **PKCE (S256)**, `state` and `nonce`
- **Client Credentials** flow for service-to-service tokens
- **Refresh Token** flow
- **UserInfo** and **Logout** endpoints
- **User picker login page** – choose any seeded user, no password needed
- **Keycloak-style Admin API** (`/admin/realms/{realm}/...`) for users and client role mappings
- **Multiple realms** (`cvm`, `kc`, `nrm`), each with its own users and service clients

## Tech stack

- .NET 9 / ASP.NET Core MVC
- `System.IdentityModel.Tokens.Jwt` / `Microsoft.IdentityModel.*` for token creation
- JSON files as the data store

## Solution structure

```
MockIdentityProvider.slnx
├── MockIdentityProvider                  # Web host: controllers, login views, Program.cs, appsettings
├── MockIdentityProvider.Core             # Application logic: authorize, token, user and client services
├── MockIdentityProvider.Domain           # Models (User, Client, Role, ServiceClient...) and repository interfaces
└── MockIdentityProvider.Infrastructure   # JSON repositories, JWT generator, RSA key, seed data
    └── Data
        ├── Clients   # clients.json + <realm>-service-clients.json
        └── Users     # <realm>-users.json
```

## Getting started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Run

```bash
dotnet run --project MockIdentityProvider --launch-profile https
```

| Profile | URL |
|---------|-----|
| `https` | https://localhost:7090 (and http://localhost:5266) |
| `http`  | http://localhost:5266 |

Opening the root URL redirects to the login page (`/connect/authorize`).

Check it is up:

```bash
curl -k https://localhost:7090/.well-known/openid-configuration
```

## Configuration

`MockIdentityProvider/appsettings.json`:

```json
{
  "jwt": {
    "Issuer": "https://localhost:7090",
    "AccessTokenExpiryMinutes": 15,
    "IdTokenExpiryMinutes": 15
  },
  "MockIdentityProviderCore": {
    "AllowedOrigins": [ "http://localhost:5173", "https://localhost:5173" ],
    "AllowedMethods": [ "GET", "POST" ]
  }
}
```

| Setting | Description |
|---------|-------------|
| `jwt:Issuer` | The `iss` claim and the authority your apps point to. Must match the URL the IdP runs on. |
| `jwt:AccessTokenExpiryMinutes` | Access token lifetime. |
| `jwt:IdTokenExpiryMinutes` | ID token lifetime. |
| `MockIdentityProviderCore:AllowedOrigins` | CORS origins allowed to call the IdP (e.g. your SPA). |
| `MockIdentityProviderCore:AllowedMethods` | CORS methods allowed. |

## Endpoints

### OpenID Connect

| Method | Path | Description |
|--------|------|-------------|
| GET | `/.well-known/openid-configuration` | Discovery document |
| GET | `/.well-known/jwks.json` | Public signing keys |
| GET | `/connect/authorize` | Authorization endpoint / login page (user picker) |
| POST | `/connect/token` | Token endpoint (`authorization_code`, `client_credentials`, `refresh_token`) |
| GET, POST | `/connect/userinfo` | User info for the bearer token |
| GET | `/connect/logout` | Ends the session |

### Admin API (Keycloak-compatible)

| Method | Path | Description |
|--------|------|-------------|
| GET | `/admin/realms/{realm}/users` | Search / list users |
| GET | `/admin/realms/{realm}/users/{id}` | Get user |
| POST | `/admin/realms/{realm}/users` | Create user |
| PUT | `/admin/realms/{realm}/users/{id}` | Update user |
| DELETE | `/admin/realms/{realm}/users/{id}` | Delete user |
| PUT | `/admin/realms/{realm}/users/{id}/reset-password` | Reset password (accepted, value ignored) |
| GET / POST / DELETE | `/admin/realms/{realm}/users/{id}/role-mappings/clients/{clientUuid}` | Read / add / remove client roles |
| GET | `/admin/realms/{realm}/clients/{clientId}/roles/{roleName}` | Get a client role |
| GET | `/admin/realms/{realm}/clients/{clientId}/roles/{roleName}/users` | Users that have a role |
| GET | `/admin/realms/{realm}/events` | Always returns `[]` |

> Admin API changes are written to the `<realm>-users.json` file in the **build output** folder, so they last
> until the next rebuild.

## Usage examples

### Authorization Code + PKCE (browser apps)

1. Redirect the user to:

   ```
   https://localhost:7090/connect/authorize
     ?client_id=kc-app
     &redirect_uri=http://localhost:5173
     &response_type=code
     &scope=openid
     &state=xyz
     &code_challenge=<S256 challenge>
     &code_challenge_method=S256
   ```

2. Pick a user on the login page. You are redirected back with `?code=...&state=xyz`.
3. Exchange the code:

   ```bash
   curl -k -X POST https://localhost:7090/connect/token \
     -d grant_type=authorization_code \
     -d client_id=kc-app \
     -d code=<code> \
     -d redirect_uri=http://localhost:5173 \
     -d code_verifier=<verifier>
   ```

Standard OIDC client libraries (`oidc-client-ts`, `keycloak-js`, ASP.NET Core OpenIdConnect) work by setting the
authority to `https://localhost:7090`.

### Client Credentials (service-to-service)

```bash
curl -k -X POST https://localhost:7090/connect/token \
  -d grant_type=client_credentials \
  -d client_id=<service client id> \
  -d client_secret=<service client secret>
```

### Refresh token

```bash
curl -k -X POST https://localhost:7090/connect/token \
  -d grant_type=refresh_token \
  -d client_id=kc-app \
  -d refresh_token=<refresh token>
```

### Validate tokens in your API

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://localhost:7090";
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.RoleClaimType = "roles";
    });
```

## Seed data

All data is in `MockIdentityProvider.Infrastructure/Data` and is copied to the build output.

### Clients – `Data/Clients/clients.json`

Each browser client maps to one realm and its data files:

```json
{
  "clientId": "kc-app",
  "realm": "kc",
  "redirectUrls": [ "http://localhost:5173", "http://localhost:5173/*" ],
  "rolesClient": "web-api-client",
  "userFile": "kc-users.json",
  "systemName": "...",
  "serviceClientFile": "kc-service-clients.json"
}
```

| Client ID | Realm |
|-----------|-------|
| `cvm-app` | `cvm` |
| `kc-app`  | `kc`  |
| `nrm-app` | `nrm` |

### Service clients – `Data/Clients/<realm>-service-clients.json`

Clients for the `client_credentials` flow (`clientId`, `clientSecret`, `audiences`, `scopes`, ...).

### Users – `Data/Users/<realm>-users.json`

```json
{
  "Id": "1363133a-a715-4f94-b1f0-a7571fa06c18",
  "Username": "manager6",
  "FirstName": "manager",
  "LastName": "6",
  "Email": "",
  "Roles": [ { "ClientId": "web-api-client", "Name": "Manager" } ],
  "Attributes": {
    "Department": [ "6" ],
    "Agency": [ "2" ],
    "EmployeeNumber": [ "" ]
  }
}
```

### Adding a new realm

1. Add `Data/Users/<realm>-users.json` and `Data/Clients/<realm>-service-clients.json`.
2. Add a client entry for the realm in `Data/Clients/clients.json`.
3. Copy the new files to the output in `MockIdentityProvider.Infrastructure.csproj` (`CopyToOutputDirectory`).

## Notes and limitations

- **No passwords** – any listed user can sign in.
- **New signing key on every start** – the RSA key is created in memory at startup, so tokens issued before a
  restart no longer validate. Sign in again after restarting.
- **In-memory state** – authorization codes and pending login requests are lost on restart.
- **Not for production.**

## Build

```bash
dotnet build MockIdentityProvider.slnx
```
