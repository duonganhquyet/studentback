namespace StudentAcademicManagement.Application.DTOs.Students
{
    public class StudentListResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsFirstLogin { get; set; }

        // TRƯỜNG DỮ LIỆU CÒN THIẾU CẦN BỔ SUNG ĐỂ SỬA LỖI
        public string AcademicStatus { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string FacultyName { get; set; } = string.Empty;
        public string MajorName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}