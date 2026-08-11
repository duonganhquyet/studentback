namespace StudentAcademicManagement.Application.DTOs.Students
{
    public class FamilyMemberDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public string? Nationality { get; set; }
        public string? BirthYear { get; set; }
        public string? Job { get; set; }
        public string? Position { get; set; }
        public string? Company { get; set; }
        public string? Ethnicity { get; set; }
        public string? Religion { get; set; }
        public string? Phone { get; set; }
        public string? PermanentAddress { get; set; }
        public string? CurrentAddress { get; set; }
        public bool IsEmergencyContact { get; set; }
        public bool IsAlumni { get; set; }
    }
}
