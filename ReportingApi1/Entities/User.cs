namespace ReportingApi1.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public Company Company { get; set; } = null!;
        public UserRole Role { get; set; } = UserRole.User;

    }

    public enum UserRole
    {
        User,
        Admin,
    }
}

