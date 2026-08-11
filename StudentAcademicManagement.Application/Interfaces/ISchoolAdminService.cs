using StudentAcademicManagement.Application.DTOs.SchoolAdmins;

namespace StudentAcademicManagement.Application.Interfaces
{
    public interface ISchoolAdminService
    {
        Task<SchoolAdminResponse> CreateSchoolAdminAsync(CreateSchoolAdminRequest request);
        Task<IEnumerable<SchoolAdminResponse>> GetAllSchoolAdminsAsync();
    }
}