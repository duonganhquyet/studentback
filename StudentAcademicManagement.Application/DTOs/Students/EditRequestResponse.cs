namespace StudentAcademicManagement.Application.DTOs.Students
{
    public class EditRequestResponse
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string RequestedFullName { get; set; } = string.Empty;
        public DateTime? RequestedDateOfBirth { get; set; }
        public string? RequestedGender { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? AdminComment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}