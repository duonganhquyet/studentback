using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace StudentAcademicManagement.Application.DTOs.Students
{
    public class UploadDocumentRequest
    {
        [Required(ErrorMessage = "Tên tài liệu là bắt buộc")]
        public string DocumentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại tài liệu là bắt buộc")]
        public string DocumentType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn file tải lên")]
        public IFormFile File { get; set; } = null!;
    }
}