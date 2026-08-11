namespace StudentAcademicManagement.Domain.Entities
{
    public class StudentEditRequest : BaseEntity
    {
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public string RequestedFullName { get; set; } = string.Empty;
        public DateTime? RequestedDateOfBirth { get; set; }
        public string? RequestedGender { get; set; }

        public string Reason { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        public string? AdminComment { get; set; }
    }
}