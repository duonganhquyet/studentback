using StudentAcademicManagement.Application.DTOs.Common;
using StudentAcademicManagement.Application.DTOs.Students;

namespace StudentAcademicManagement.Application.Interfaces
{
    public interface IStudentService
    {
        Task<StudentResponse> CreateStudentAsync(int schoolId, CreateStudentRequest request);
        Task<PagedResult<StudentListResponse>> GetStudentsAsync(int schoolId, StudentFilterRequest request);

        Task ChangeStudentStatusAsync(int schoolId, int studentId, int adminUserId, ChangeStudentStatusRequest request);

        Task UpdateStudentAcademicInfoAsync(int schoolId, int studentId, UpdateStudentAcademicRequest request);

        // Nhập danh sách sinh viên theo lô (Excel/CSV batch import)
        Task<BatchImportResult> BatchImportStudentsAsync(int schoolId, List<BatchImportStudentItem> items);
    }
}