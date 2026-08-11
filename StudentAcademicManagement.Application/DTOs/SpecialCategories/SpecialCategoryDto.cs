using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace StudentAcademicManagement.Application.DTOs.SpecialCategories
{
    public class SpecialCategoryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateSpecialCategoryRequest
    {
        [Required(ErrorMessage = "Tên đối tượng là bắt buộc")]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class StudentSpecialCategoryResponse
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty; // Join từ Profile
        public int SpecialCategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string ProofFileUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RegisterSpecialCategoryRequest
    {
        [Required]
        public int SpecialCategoryId { get; set; }

        [Required(ErrorMessage = "Vui lòng tải lên minh chứng")]
        public IFormFile ProofFile { get; set; } = null!;
    }

    public class ReviewSpecialCategoryRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty; // Approved / Rejected
        public string? RejectionReason { get; set; }
    }
}