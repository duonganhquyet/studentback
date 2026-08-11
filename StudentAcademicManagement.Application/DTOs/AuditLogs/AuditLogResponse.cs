namespace StudentAcademicManagement.Application.DTOs.AuditLogs
{
    public class AuditLogResponse
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty; // Lấy từ bảng User

        public int? SchoolId { get; set; }
        public string SchoolName { get; set; } = string.Empty; // Lấy từ bảng School

        public string Action { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;

        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? IpAddress { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}