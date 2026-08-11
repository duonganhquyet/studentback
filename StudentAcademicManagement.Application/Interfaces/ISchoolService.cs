using StudentAcademicManagement.Application.DTOs.Schools;

namespace StudentAcademicManagement.Application.Interfaces
{
    public interface ISchoolService
    {
        Task<SchoolResponse> CreateSchoolAsync(CreateSchoolRequest request);
        Task<IEnumerable<SchoolResponse>> GetAllSchoolsAsync();
        Task<SchoolResponse> GetSchoolByIdAsync(int id);
        Task<SchoolResponse> UpdateSchoolAsync(int id, UpdateSchoolRequest request);
        Task ChangeSchoolStatusAsync(int id, bool isActive);
    }
}