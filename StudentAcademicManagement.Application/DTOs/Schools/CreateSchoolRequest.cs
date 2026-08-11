using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace StudentAcademicManagement.Application.DTOs.Schools
{
    public class CreateSchoolRequest
    {
        [Required(ErrorMessage = "Mã trường là bắt buộc")]
        public string SchoolCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên trường là bắt buộc")]
        public string SchoolName { get; set; } = string.Empty;

        public string? ShortName { get; set; }

        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Website { get; set; }
        public string? Description { get; set; }

        // Nhận file Logo từ form-data
        public IFormFile? Logo { get; set; }
    }
}