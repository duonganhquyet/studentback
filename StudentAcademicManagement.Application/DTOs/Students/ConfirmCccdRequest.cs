using StudentAcademicManagement.Application.Interfaces;

namespace StudentAcademicManagement.Application.DTOs.Students
{
    public class ConfirmCccdRequest
    {
        public string IdNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime? Dob { get; set; }
        public string Gender { get; set; } = string.Empty;
        public DateTime? IssueDate { get; set; }
        public string? IssuePlace { get; set; }
        public string? PlaceOfOrigin { get; set; }
        public string? Address { get; set; }
        
        public List<FamilyMemberDto> FamilyMembers { get; set; } = new List<FamilyMemberDto>();
    }
}