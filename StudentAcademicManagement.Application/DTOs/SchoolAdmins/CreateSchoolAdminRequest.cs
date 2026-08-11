using System.ComponentModel.DataAnnotations;

namespace StudentAcademicManagement.Application.DTOs.SchoolAdmins
{
    public class CreateSchoolAdminRequest
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu tạm thời là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên")]
        public string TemporaryPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phải chọn trường học (SchoolId)")]
        public int SchoolId { get; set; }
    }
}