namespace MockIdentityProvider.Core
{
    public class LogoutRequest
    {
        public string? id_token_hint { get; set; }

        public string? post_logout_redirect_uri { get; set; }

        public string? state { get; set; }

        public string? client_id { get; set; }
    }
}
