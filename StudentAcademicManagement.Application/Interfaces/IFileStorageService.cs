using Microsoft.AspNetCore.Http;

namespace StudentAcademicManagement.Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, string folderName);
    }
}