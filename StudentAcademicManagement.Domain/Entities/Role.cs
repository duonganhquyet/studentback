public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty; // SuperAdmin, SchoolAdmin, Student
    public ICollection<User> Users { get; set; } = new List<User>();
}