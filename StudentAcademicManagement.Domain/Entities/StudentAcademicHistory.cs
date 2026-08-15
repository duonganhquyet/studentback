namespace StudentAcademicManagement.Domain.Entities
{
    public class StudentAcademicHistory : BaseEntity
    {
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;
        public string? FromTime { get; set; }
        public string? ToTime { get; set; }
        public string? SchoolName { get; set; }
        public string? Notes { get; set; }
    }
}
