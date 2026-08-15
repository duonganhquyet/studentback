using System.ComponentModel.DataAnnotations;

namespace StudentAcademicManagement.Application.DTOs.Students
{
    public class CreateStudentRequest
    {
        [Required(ErrorMessage = "Mã sinh viên là bắt buộc")]
        public string StudentCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu tạm thời là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên")]
        public string TemporaryPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên Khoa là bắt buộc")]
        public string FacultyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên Ngành là bắt buộc")]
        public string MajorName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên Lớp quản lý là bắt buộc")]
        public string ClassName { get; set; } = string.Empty;

        // Expanded Profile & Identity Information
        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        public string FullName { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        public string Gender { get; set; } = "Nam"; // Nam, Nữ, Khác

        // [Required(ErrorMessage = "Số CCCD là bắt buộc")]
        public string IdNumber { get; set; } = string.Empty;

        public DateTime? IssueDate { get; set; }
        public string IssuePlace { get; set; } = string.Empty;

        // Regional Info
        public string Province { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string RegionType { get; set; } = string.Empty;
    }
}