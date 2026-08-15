using System.Collections.Generic;
using System.Threading.Tasks;
using StudentAcademicManagement.Application.DTOs.Students;

namespace StudentAcademicManagement.Application.Interfaces
{
    public interface IPaperRequestService
    {
        // Sinh viên
        Task<IEnumerable<PaperRequestResponse>> GetMyPaperRequestsAsync(int userId);
        Task<PaperRequestResponse> CreatePaperRequestAsync(int userId, CreatePaperRequest request);
        Task DeleteMyPaperRequestAsync(int userId, int requestId);

        // Admin
        Task<IEnumerable<PaperRequestResponse>> GetAllPaperRequestsAsync(int schoolId);
        Task ReviewPaperRequestAsync(int schoolId, int requestId, ReviewPaperRequest request);
    }
}
