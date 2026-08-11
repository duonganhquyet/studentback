namespace StudentAcademicManagement.Application.DTOs.Students
{
    public class StudentProfileResponse
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Ethnicity { get; set; }
        public string? Nationality { get; set; }
        public string? PlaceOfBirth { get; set; }
        public string? AvatarUrl { get; set; }

        public string FacultyName { get; set; } = string.Empty;
        public string MajorName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string AcademicStatus { get; set; } = string.Empty;
    }
}