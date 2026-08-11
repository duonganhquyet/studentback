using Microsoft.EntityFrameworkCore;
using StudentAcademicManagement.Application.DTOs.Students;
using StudentAcademicManagement.Application.Interfaces;
using StudentAcademicManagement.Domain.Entities;
using StudentAcademicManagement.Infrastructure.Persistence;

namespace StudentAcademicManagement.Infrastructure.Services
{
    public class StudentDocumentService : IStudentDocumentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorageService;

        public StudentDocumentService(ApplicationDbContext context, IFileStorageService fileStorageService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
        }

        // --- HÀM CHO STUDENT ---
        public async Task<StudentDocumentResponse> UploadDocumentAsync(int userId, UploadDocumentRequest request)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) throw new UnauthorizedAccessException("Tài khoản không phải là sinh viên.");

            var fileUrl = await _fileStorageService.SaveFileAsync(request.File, "documents");

            var doc = new StudentDocument
            {
                StudentId = student.Id,
                DocumentName = request.DocumentName,
                DocumentType = request.DocumentType,
                FileUrl = fileUrl,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.StudentDocuments.Add(doc);
            await _context.SaveChangesAsync();

            return MapToResponse(doc);
        }

        public async Task<IEnumerable<StudentDocumentResponse>> GetMyDocumentsAsync(int userId)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) throw new UnauthorizedAccessException("Tài khoản không hợp lệ.");

            var docs = await _context.StudentDocuments
                .Where(d => d.StudentId == student.Id)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return docs.Select(MapToResponse);
        }

        public async Task DeleteMyDocumentAsync(int userId, int documentId)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) throw new UnauthorizedAccessException("Tài khoản không hợp lệ.");

            var doc = await _context.StudentDocuments.FirstOrDefaultAsync(d => d.Id == documentId && d.StudentId == student.Id);
            if (doc == null) throw new KeyNotFoundException("Không tìm thấy tài liệu của bạn.");
            if (doc.Status == "Approved") throw new InvalidOperationException("Không thể xóa tài liệu đã được phê duyệt.");

            _context.StudentDocuments.Remove(doc);
            await _context.SaveChangesAsync();
        }

        // --- HÀM CHO SCHOOL ADMIN ---
        public async Task<IEnumerable<StudentDocumentResponse>> GetDocumentsByStudentIdAsync(int schoolId, int studentId)
        {
            // Bảo mật Multi-School: Chắc chắn sinh viên này thuộc trường của Admin
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == studentId && s.SchoolId == schoolId);
            if (student == null) throw new KeyNotFoundException("Không tìm thấy sinh viên trong trường của bạn.");

            var docs = await _context.StudentDocuments
                .Where(d => d.StudentId == studentId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return docs.Select(MapToResponse);
        }

        public async Task ReviewDocumentAsync(int schoolId, int documentId, ReviewDocumentRequest request)
        {
            var doc = await _context.StudentDocuments
                .Include(d => d.Student)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (doc == null) throw new KeyNotFoundException("Không tìm thấy tài liệu.");
            if (doc.Student.SchoolId != schoolId) throw new UnauthorizedAccessException("Tài liệu này không thuộc sinh viên trường của bạn.");

            if (request.Status != "Approved" && request.Status != "Rejected")
                throw new ArgumentException("Trạng thái duyệt không hợp lệ.");

            if (request.Status == "Rejected" && string.IsNullOrWhiteSpace(request.RejectionReason))
                throw new ArgumentException("Phải nhập lý do từ chối.");

            doc.Status = request.Status;
            doc.RejectionReason = request.Status == "Rejected" ? request.RejectionReason : null;
            doc.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteDocumentAsync(int schoolId, int documentId)
        {
            var doc = await _context.StudentDocuments
                .Include(d => d.Student)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (doc == null) throw new KeyNotFoundException("Không tìm thấy tài liệu.");
            if (doc.Student.SchoolId != schoolId) throw new UnauthorizedAccessException("Tài liệu này không thuộc sinh viên trường của bạn.");

            _context.StudentDocuments.Remove(doc);
            await _context.SaveChangesAsync();
        }

        private StudentDocumentResponse MapToResponse(StudentDocument doc)
        {
            return new StudentDocumentResponse
            {
                Id = doc.Id,
                StudentId = doc.StudentId,
                DocumentName = doc.DocumentName,
                DocumentType = doc.DocumentType,
                FileUrl = doc.FileUrl,
                Status = doc.Status,
                RejectionReason = doc.RejectionReason,
                CreatedAt = doc.CreatedAt
            };
        }
    }
}