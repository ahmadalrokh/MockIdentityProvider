namespace MockIdentityProvider.Core.Clients.Dtos
{
    public record ClientContext
    {
        public required string ClientId { get; set; }
        public required string RedirectUrl { get; set; }
    }
}
