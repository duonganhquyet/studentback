using StudentAcademicManagement.Application.DTOs.Students;

namespace StudentAcademicManagement.Application.Interfaces
{
    public interface IStudentProfileService
    {
        Task<StudentProfileResponse> GetProfileByStudentIdAsync(int schoolId, int studentId);
        Task<StudentProfileResponse> UpdateProfileAsync(int schoolId, int studentId, UpdateStudentProfileRequest request);
    }
}