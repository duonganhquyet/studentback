using System.ComponentModel.DataAnnotations;

namespace StudentAcademicManagement.Application.DTOs.Students
{
    public class ChangeStudentStatusRequest
    {
        [Required(ErrorMessage = "Vui lòng chọn trạng thái mới")]
        public string NewStatus { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bắt buộc phải nhập lý do thay đổi trạng thái")]
        public string Reason { get; set; } = string.Empty;
    }
}