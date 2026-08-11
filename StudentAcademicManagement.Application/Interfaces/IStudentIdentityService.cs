using StudentAcademicManagement.Application.DTOs.Students;

namespace StudentAcademicManagement.Application.Interfaces
{
    public interface IStudentIdentityService
    {
        Task<CccdComparisonResponse> UploadAndProcessCccdAsync(int userId, UploadCccdRequest request);
        Task ConfirmAndLockIdentityAsync(int userId, ConfirmCccdRequest request);
        Task<StudentIdentityResponse?> GetIdentityAsync(int userId);
        Task<EditRequestResponse> CreateEditRequestAsync(int userId, CreateEditRequest request);
        Task<EditRequestResponse?> GetMyPendingEditRequestAsync(int userId);
        Task<IEnumerable<EditRequestResponse>> GetPendingEditRequestsAsync(int schoolId);
        Task ReviewEditRequestAsync(int schoolId, int requestId, int adminUserId, ReviewEditRequest request);
    }

    public class StudentIdentityResponse
    {
        public string? StudentCode { get; set; }
        public string? IdNumber { get; set; }
        public string? FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? PlaceOfOrigin { get; set; }
        public string? PlaceOfResidence { get; set; }
        public DateTime? IssueDate { get; set; }
        public string? IssuePlace { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
        public string? FrontImageUrl { get; set; }
        public string? BackImageUrl { get; set; }
    }
}