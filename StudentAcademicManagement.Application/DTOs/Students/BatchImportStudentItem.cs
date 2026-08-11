using System.ComponentModel.DataAnnotations;

namespace StudentAcademicManagement.Application.DTOs.Students
{
    public class BatchImportStudentItem
    {
        [Required(ErrorMessage = "MSSV là bắt buộc")]
        public string StudentCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        public string? FacultyName { get; set; }
        public string? MajorName { get; set; }
        public string? ClassName { get; set; }
    }

    public class BatchImportResult
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> ErrorMessages { get; set; } = new List<string>();
    }
}
