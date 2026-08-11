using System.ComponentModel.DataAnnotations;

namespace StudentAcademicManagement.Application.DTOs.Students
{
    public class CreateEditRequest
    {
        [Required(ErrorMessage = "Họ tên mới là bắt buộc")]
        public string RequestedFullName { get; set; } = string.Empty;

        public DateTime? RequestedDateOfBirth { get; set; }
        public string? RequestedGender { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do xin sửa đổi")]
        public string Reason { get; set; } = string.Empty;
    }
}