public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public int? SchoolId { get; set; } // Null for SuperAdmin, Required for others
    public School? School { get; set; }

    public bool IsFirstLogin { get; set; } = true;
    public bool IsActive { get; set; } = true;
}