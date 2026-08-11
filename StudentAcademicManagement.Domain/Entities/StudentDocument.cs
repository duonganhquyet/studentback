namespace StudentAcademicManagement.Domain.Entities
{
    public class StudentDocument : BaseEntity
    {
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public string DocumentName { get; set; } = string.Empty; // VD: Giấy khai sinh, Bằng tốt nghiệp
        public string DocumentType { get; set; } = string.Empty; // VD: Identity, Academic, Health, Other
        public string FileUrl { get; set; } = string.Empty; // Đường dẫn file vật lý

        // Trạng thái: Pending, Approved, Rejected
        public string Status { get; set; } = "Pending";

        // Lý do nếu bị Admin từ chối
        public string? RejectionReason { get; set; }
    }
}