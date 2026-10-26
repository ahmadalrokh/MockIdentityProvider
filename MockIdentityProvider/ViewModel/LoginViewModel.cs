namespace MockIdentityProvider.ViewModel
{
    public class LoginViewModel
    {
        public string AuthzId { get; set; } = string.Empty;
        public string SystemName { get; set; } = string.Empty;
        public List<UserList> UserList { get; set; } = new();
        public string SelectedUserId { get; set; } = string.Empty;
    }
    public class UserList
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string fullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;

    }

}
