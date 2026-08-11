using StudentAcademicManagement.Application.DTOs.Auth;

namespace StudentAcademicManagement.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<string> ChangePasswordAsync(int userId, ChangePasswordRequest request);
    }
}