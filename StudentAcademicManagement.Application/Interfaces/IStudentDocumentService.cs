using StudentAcademicManagement.Application.DTOs.Students;

namespace StudentAcademicManagement.Application.Interfaces
{
    public interface IStudentDocumentService
    {
        // Cho Student: Upload tài liệu
        Task<StudentDocumentResponse> UploadDocumentAsync(int userId, UploadDocumentRequest request);

        // Cho Student: Xem tài liệu của chính mình
        Task<IEnumerable<StudentDocumentResponse>> GetMyDocumentsAsync(int userId);

        // Cho Student: Xóa tài liệu của mình (chỉ khi chưa được duyệt)
        Task DeleteMyDocumentAsync(int userId, int documentId);

        // Cho SchoolAdmin: Xem tài liệu của 1 sinh viên cụ thể
        Task<IEnumerable<StudentDocumentResponse>> GetDocumentsByStudentIdAsync(int schoolId, int studentId);

        // Cho SchoolAdmin: Duyệt / Từ chối tài liệu
        Task ReviewDocumentAsync(int schoolId, int documentId, ReviewDocumentRequest request);

        // Cho SchoolAdmin: Xóa tài liệu lỗi/trùng
        Task DeleteDocumentAsync(int schoolId, int documentId);
    }
}