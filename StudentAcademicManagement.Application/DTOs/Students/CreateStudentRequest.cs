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
    }
}