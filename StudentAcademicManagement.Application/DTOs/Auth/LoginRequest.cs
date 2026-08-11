using System.ComponentModel.DataAnnotations;

namespace StudentAcademicManagement.Application.DTOs.Auth
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string Password { get; set; } = string.Empty;
    }
}