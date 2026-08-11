using StudentAcademicManagement.Application.DTOs.Dashboards;

namespace StudentAcademicManagement.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<SuperAdminDashboardResponse> GetSuperAdminDashboardAsync();
        Task<SchoolAdminDashboardResponse> GetSchoolAdminDashboardAsync(int schoolId);
        Task<StudentDashboardResponse> GetStudentDashboardAsync(int userId);
    }
}