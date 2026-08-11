public class School : BaseEntity
{
    public string SchoolCode { get; set; } = string.Empty;
    public string SchoolName { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? LogoUrl { get; set; }
    public string? Website { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<User> Users { get; set; } = new List<User>();
    // Navigation for Students will be added in later tasks
}