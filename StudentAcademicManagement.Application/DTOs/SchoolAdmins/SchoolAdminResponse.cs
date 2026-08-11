namespace StudentAcademicManagement.Application.DTOs.SchoolAdmins
{
    public class SchoolAdminResponse
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public int SchoolId { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}