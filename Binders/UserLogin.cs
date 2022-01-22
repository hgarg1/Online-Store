namespace Binders
{
    public class UserLogin //used when a login request is intiated, form data is bound to C# fields
    {
        public string username { get; set; }
        public string? password { get; set; }
        public string? lastLogin { get; set; }
    }
}