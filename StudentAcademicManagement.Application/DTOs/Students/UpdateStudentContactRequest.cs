using System.ComponentModel.DataAnnotations;

namespace StudentAcademicManagement.Application.DTOs.Students
{
    public class UpdateStudentContactRequest
    {
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? TemporaryAddress { get; set; }
        public string? ResidenceType { get; set; }
        public string? LandlordName { get; set; }
        public string? LandlordPhone { get; set; }

        public string? GuardianName { get; set; }
        public string? GuardianPhoneNumber { get; set; }
        public string? GuardianRelationship { get; set; }

        public List<FamilyMemberDto> FamilyMembers { get; set; } = new List<FamilyMemberDto>();
    }
}