using StudentAcademicManagement.Application.DTOs.SpecialCategories;

namespace StudentAcademicManagement.Application.Interfaces
{
    public interface ISpecialCategoryService
    {
        // Admin quản lý danh mục
        Task<IEnumerable<SpecialCategoryResponse>> GetCategoriesAsync(int schoolId);
        Task<SpecialCategoryResponse> CreateCategoryAsync(int schoolId, CreateSpecialCategoryRequest request);
        Task ToggleCategoryStatusAsync(int schoolId, int id);

        // Sinh viên đăng ký
        Task<StudentSpecialCategoryResponse> RegisterSpecialCategoryAsync(int userId, RegisterSpecialCategoryRequest request);
        Task<IEnumerable<StudentSpecialCategoryResponse>> GetMyRegistrationsAsync(int userId);

        // Admin duyệt
        Task<IEnumerable<StudentSpecialCategoryResponse>> GetPendingRegistrationsAsync(int schoolId);
        Task ReviewRegistrationAsync(int schoolId, int registrationId, ReviewSpecialCategoryRequest request);
    }
}