namespace StudentAcademicManagement.Application.DTOs.Students
{
    public class StudentContactResponse
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? TemporaryAddress { get; set; }
        public string? GuardianName { get; set; }
        public string? GuardianPhoneNumber { get; set; }
        public string? GuardianRelationship { get; set; }
    }
}