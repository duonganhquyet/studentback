using System.ComponentModel.DataAnnotations;

namespace StudentAcademicManagement.Application.DTOs.Students
{
    public class ReviewDocumentRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty; // "Approved" hoặc "Rejected"

        public string? RejectionReason { get; set; }
    }
}