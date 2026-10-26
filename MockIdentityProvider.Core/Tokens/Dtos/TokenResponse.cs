using System.Text.Json.Serialization;

namespace MockIdentityProvider.Core.Tokens.Dtos
{
    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = default!;

        [JsonPropertyName("id_token")]
        public string IdToken { get; set; } = default!;

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = "Bearer";

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_expires_in")]
        public int RefreshExpiresIn { get; set; } = 1800;

        [JsonPropertyName("session_state")]
        public string? SessionState { get; set; }
    }
}
