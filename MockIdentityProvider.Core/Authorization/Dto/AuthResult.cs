namespace MockIdentityProvider.Domain.Models
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? RedirectUrl { get; set; }
    }
}
