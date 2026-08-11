using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace StudentAcademicManagement.Application.DTOs.Students
{
    public class UploadCccdRequest
    {
        [Required(ErrorMessage = "Vui lòng tải lên mặt trước CCCD")]
        public IFormFile FrontImage { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng tải lên mặt sau CCCD")]
        public IFormFile BackImage { get; set; } = null!;
    }
}