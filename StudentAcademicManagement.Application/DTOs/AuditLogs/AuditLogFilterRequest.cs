namespace StudentAcademicManagement.Application.DTOs.AuditLogs
{
    public class AuditLogFilterRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SearchTerm { get; set; } // Tìm theo Action, EntityName
    }
}