namespace MockIdentityProvider.Core.Users.Dtos
{
    public sealed class UserSearchCriteria
    {
        public string? Search { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public bool? IsActive { get; set; }
        public int Offset { get; set; }
        public int Limit { get; set; } = 100;
    }
}
