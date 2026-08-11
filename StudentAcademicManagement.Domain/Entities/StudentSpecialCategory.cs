namespace StudentAcademicManagement.Domain.Entities
{
    public class StudentSpecialCategory : BaseEntity
    {
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public int SpecialCategoryId { get; set; }
        public SpecialCategory SpecialCategory { get; set; } = null!;

        public string ProofFileUrl { get; set; } = string.Empty; // File minh chứng
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        public string? RejectionReason { get; set; }
    }
}