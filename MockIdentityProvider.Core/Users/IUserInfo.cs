namespace MockIdentityProvider.Core.Users
{
    public interface IUserInfo
    {
        Task<UserDto> GetUserInfoAsync(string accessToken);
    }
}
