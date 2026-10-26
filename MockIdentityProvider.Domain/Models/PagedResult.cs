namespace MockIdentityProvider.Domain.Models
{
    public sealed class PagedResult<T>
    {
        public List<T> Data { get; init; } = [];
        public int TotalCount { get; init; }
    }
}
