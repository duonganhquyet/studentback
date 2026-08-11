using StudentAcademicManagement.Application.DTOs.AuditLogs;
using StudentAcademicManagement.Application.DTOs.Common;

namespace StudentAcademicManagement.Application.Interfaces
{
    public interface IAuditLogService
    {
        // Cho Super Admin (Xem toàn hệ thống)
        Task<PagedResult<AuditLogResponse>> GetSystemAuditLogsAsync(AuditLogFilterRequest request);

        // Cho School Admin (Chỉ xem của trường mình)
        Task<PagedResult<AuditLogResponse>> GetSchoolAuditLogsAsync(int schoolId, AuditLogFilterRequest request);

        // Hàm ghi log hành động
        Task LogAsync(int? userId, int? schoolId, string action, string entityName, string? entityId = null, string? oldValue = null, string? newValue = null);
    }
}