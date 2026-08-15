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

        // Academic Info
        public string? FacultyName { get; set; }
        public string? MajorName { get; set; }
        public string? ClassName { get; set; }

        // Profile Info
        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        public string FullName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? PlaceOfBirth { get; set; }
        public string? Ethnicity { get; set; }
        public string? Nationality { get; set; }
        public string? RegionType { get; set; }

        // Contact Info
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? TemporaryAddress { get; set; }
        public string? GuardianName { get; set; }
        public string? GuardianPhoneNumber { get; set; }
        public string? GuardianRelationship { get; set; }
        public string? ResidenceType { get; set; }
        public string? LandlordName { get; set; }
        public string? LandlordPhone { get; set; }
    }

    public class BatchImportResult
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> ErrorMessages { get; set; } = new List<string>();
    }
}
