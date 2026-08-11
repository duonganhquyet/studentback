namespace StudentAcademicManagement.Domain.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int? UserId { get; set; } // Người thực hiện hành động
        public int? SchoolId { get; set; }
        public string Action { get; set; } = string.Empty; // Ví dụ: "Approve CCCD Edit Request"
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string? OldValue { get; set; } // Chuỗi JSON lưu giá trị cũ
        public string? NewValue { get; set; } // Chuỗi JSON lưu giá trị mới
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}