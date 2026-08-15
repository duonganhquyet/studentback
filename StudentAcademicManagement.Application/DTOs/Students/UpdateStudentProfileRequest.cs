using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace StudentAcademicManagement.Application.DTOs.Students
{
    public class UpdateStudentProfileRequest
    {
        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        public string FullName { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Ethnicity { get; set; }
        public string? Nationality { get; set; }
        public string? PlaceOfBirth { get; set; }

        public IFormFile? Avatar { get; set; }

        public bool HasBeenClassMonitor { get; set; }
        public bool HasBeenYouthUnionOfficer { get; set; }
        public bool HasParticipatedInExcellentStudentTeam { get; set; }
        public string? AwardDetails { get; set; }
        public string? CurrentRoleInSchool { get; set; }
        public string? AcademicHistoriesJson { get; set; } // JSON string for ease of FormData
    }
}