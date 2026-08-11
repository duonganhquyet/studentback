namespace StudentAcademicManagement.Application.DTOs.Students
{
    public class StudentDocumentResponse
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string DocumentName { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}