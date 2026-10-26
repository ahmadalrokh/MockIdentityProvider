namespace MockIdentityProvider.Models
{
    using System.Text.Json.Serialization;

    public sealed class OpenIdConfigurationResponse
    {
        [JsonPropertyName("issuer")]
        public string Issuer { get; set; } = string.Empty;

        [JsonPropertyName("authorization_endpoint")]
        public string AuthorizationEndpoint { get; set; } = string.Empty;

        [JsonPropertyName("token_endpoint")]
        public string TokenEndpoint { get; set; } = string.Empty;

        [JsonPropertyName("userinfo_endpoint")]
        public string UserInfoEndpoint { get; set; } = string.Empty;

        [JsonPropertyName("end_session_endpoint")]
        public string EndSessionEndpoint { get; set; } = string.Empty;

        [JsonPropertyName("jwks_uri")]
        public string JwksUri { get; set; } = string.Empty;

        [JsonPropertyName("response_types_supported")]
        public string[] ResponseTypesSupported { get; set; } = [];

        [JsonPropertyName("grant_types_supported")]
        public string[] GrantTypesSupported { get; set; } = [];

        [JsonPropertyName("subject_types_supported")]
        public string[] SubjectTypesSupported { get; set; } = [];

        [JsonPropertyName("id_token_signing_alg_values_supported")]
        public string[] IdTokenSigningAlgValuesSupported { get; set; } = [];

        [JsonPropertyName("token_endpoint_auth_methods_supported")]
        public string[] TokenEndpointAuthMethodsSupported { get; set; } = [];

        [JsonPropertyName("code_challenge_methods_supported")]
        public string[] CodeChallengeMethodsSupported { get; set; } = [];
    }
}
