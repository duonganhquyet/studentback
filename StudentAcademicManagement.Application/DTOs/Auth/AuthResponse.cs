namespace StudentAcademicManagement.Application.DTOs.Auth
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? StudentCode { get; set; }
        public int? StudentId { get; set; }
        public int? SchoolId { get; set; }
        public bool IsFirstLogin { get; set; }
    }
}