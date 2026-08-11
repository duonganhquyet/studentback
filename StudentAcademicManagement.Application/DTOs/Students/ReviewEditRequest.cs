using System.ComponentModel.DataAnnotations;

namespace StudentAcademicManagement.Application.DTOs.Students
{
    public class ReviewEditRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty; // "Approved" hoặc "Rejected"
        public string? AdminComment { get; set; }
    }
}