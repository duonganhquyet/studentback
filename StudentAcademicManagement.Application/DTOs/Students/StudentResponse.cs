namespace StudentAcademicManagement.Application.DTOs.Students
{
    public class StudentResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string FacultyName { get; set; } = string.Empty;
        public string MajorName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}