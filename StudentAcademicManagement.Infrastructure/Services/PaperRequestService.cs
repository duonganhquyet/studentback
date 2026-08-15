using Microsoft.EntityFrameworkCore;
using StudentAcademicManagement.Application.DTOs.Students;
using StudentAcademicManagement.Application.Interfaces;
using StudentAcademicManagement.Domain.Entities;
using StudentAcademicManagement.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentAcademicManagement.Infrastructure.Services
{
    // ==============================================================================
    // SERVICE XỬ LÝ YÊU CẦU CẤP GIẤY TỜ CỦA SINH VIÊN
    // ==============================================================================
    /// <summary>
    /// Nơi chứa toàn bộ logic (quy tắc nghiệp vụ) liên quan đến việc xin giấy tờ.
    /// Bao gồm: Sinh viên nộp đơn xin, Admin xem danh sách, Admin phê duyệt/từ chối và tự động gửi Email.
    /// </summary>
    public class PaperRequestService : IPaperRequestService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService; // Dùng để gọi tiến trình gửi email ngầm

        public PaperRequestService(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<IEnumerable<PaperRequestResponse>> GetMyPaperRequestsAsync(int userId)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) throw new UnauthorizedAccessException("Tài khoản không phải là sinh viên.");

            var requests = await _context.StudentPaperRequests
                .Include(r => r.Student)
                .ThenInclude(s => s.Profile)
                .Where(r => r.StudentId == student.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var specialCategories = await _context.StudentSpecialCategories
                .Include(sc => sc.SpecialCategory)
                .Where(sc => sc.StudentId == student.Id && sc.Status == "Approved")
                .Select(sc => sc.SpecialCategory.Name)
                .ToListAsync();

            return requests.Select(r => MapToResponse(r, specialCategories));
        }

        public async Task<PaperRequestResponse> CreatePaperRequestAsync(int userId, CreatePaperRequest request)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) throw new UnauthorizedAccessException("Tài khoản không phải là sinh viên.");

            // Kiểm tra trùng lặp chưa được xử lý
            var existingPending = await _context.StudentPaperRequests
                .AnyAsync(r => r.StudentId == student.Id && r.PaperType == request.PaperType && r.Status == "Pending");
            
            if (existingPending)
                throw new InvalidOperationException($"Bạn đã có yêu cầu '{request.PaperType}' đang chờ xử lý.");

            var newRequest = new StudentPaperRequest
            {
                StudentId = student.Id,
                PaperType = request.PaperType,
                Note = request.Note,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.StudentPaperRequests.Add(newRequest);
            await _context.SaveChangesAsync();

            var specialCategories = await _context.StudentSpecialCategories
                .Include(sc => sc.SpecialCategory)
                .Where(sc => sc.StudentId == student.Id && sc.Status == "Approved")
                .Select(sc => sc.SpecialCategory.Name)
                .ToListAsync();

            var savedRequest = await _context.StudentPaperRequests
                .Include(r => r.Student)
                .ThenInclude(s => s.Profile)
                .FirstAsync(r => r.Id == newRequest.Id);

            return MapToResponse(savedRequest, specialCategories);
        }

        public async Task DeleteMyPaperRequestAsync(int userId, int requestId)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) throw new UnauthorizedAccessException("Tài khoản không hợp lệ.");

            var req = await _context.StudentPaperRequests.FirstOrDefaultAsync(r => r.Id == requestId && r.StudentId == student.Id);
            if (req == null) throw new KeyNotFoundException("Không tìm thấy yêu cầu.");
            if (req.Status != "Pending") throw new InvalidOperationException("Chỉ có thể xóa yêu cầu đang chờ duyệt.");

            _context.StudentPaperRequests.Remove(req);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<PaperRequestResponse>> GetAllPaperRequestsAsync(int schoolId)
        {
            var requests = await _context.StudentPaperRequests
                .Include(r => r.Student)
                .ThenInclude(s => s.Profile)
                .Where(r => r.Student.SchoolId == schoolId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var studentIds = requests.Select(r => r.StudentId).Distinct().ToList();
            var allSpecialCategories = await _context.StudentSpecialCategories
                .Include(sc => sc.SpecialCategory)
                .Where(sc => studentIds.Contains(sc.StudentId) && sc.Status == "Approved")
                .ToListAsync();

            return requests.Select(r => 
            {
                var categories = allSpecialCategories
                    .Where(sc => sc.StudentId == r.StudentId)
                    .Select(sc => sc.SpecialCategory.Name)
                    .ToList();
                return MapToResponse(r, categories);
            });
        }

        // ==============================================================================
        // 3. DÀNH CHO ADMIN: PHÊ DUYỆT HOẶC TỪ CHỐI YÊU CẦU CỦA SINH VIÊN
        // ==============================================================================
        public async Task ReviewPaperRequestAsync(int schoolId, int requestId, ReviewPaperRequest request)
        {
            // Tìm yêu cầu dựa trên ID mà Admin gửi lên
            var req = await _context.StudentPaperRequests
                .Include(r => r.Student)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (req == null) throw new KeyNotFoundException("Không tìm thấy yêu cầu.");
            
            // Kiểm tra bảo mật chéo: Tránh việc Admin trường A lại đi duyệt đơn của sinh viên trường B
            if (req.Student.SchoolId != schoolId) throw new UnauthorizedAccessException("Yêu cầu này không thuộc sinh viên trường của bạn.");

            // Kiểm tra tính hợp lệ của dữ liệu đầu vào (Validation)
            if (request.Status != "Approved" && request.Status != "Rejected")
                throw new ArgumentException("Trạng thái duyệt không hợp lệ.");

            if (request.Status == "Rejected" && string.IsNullOrWhiteSpace(request.RejectionReason))
                throw new ArgumentException("Phải nhập lý do từ chối.");

            // Cập nhật trạng thái
            req.Status = request.Status;
            req.RejectionReason = request.Status == "Rejected" ? request.RejectionReason : null;
            req.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(); // Lưu xuống CSDL

            // ==========================================================
            // KÍCH HOẠT TIẾN TRÌNH GỬI EMAIL TỰ ĐỘNG THÔNG BÁO CHO SINH VIÊN
            // ==========================================================
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == req.Student.UserId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                string subject = $"Thông báo kết quả yêu cầu cấp giấy tờ: {req.PaperType}";
                string statusText = request.Status == "Approved" ? "ĐÃ ĐƯỢC CHẤP NHẬN và có lịch hẹn." : "ĐÃ BỊ TỪ CHỐI.";
                string reason = request.Status == "Rejected" ? $"\n- Lý do từ chối: {request.RejectionReason}" : "";
                
                string body = $@"
Xin chào {req.Student.Profile?.FullName ?? req.Student.StudentCode},

Yêu cầu xin cấp giấy tờ '{req.PaperType}' của bạn {statusText}
{reason}

Vui lòng đăng nhập vào hệ thống (mục Yêu cầu cấp giấy tờ) để xem chi tiết lịch hẹn hoặc thông tin cụ thể.

Trân trọng,
Phòng CTSV
";
                // Lệnh này trả về ngay lập tức (không làm màn hình Admin bị lag) do EmailService đã dùng Hàng đợi (Queue)
                await _emailService.SendEmailAsync(user.Email, subject, body);
            }
        }

        private PaperRequestResponse MapToResponse(StudentPaperRequest r, List<string> specialCategories)
        {
            return new PaperRequestResponse
            {
                Id = r.Id,
                StudentId = r.StudentId,
                StudentCode = r.Student?.StudentCode ?? "",
                FullName = r.Student?.Profile?.FullName ?? "",
                PaperType = r.PaperType,
                Status = r.Status,
                Note = r.Note,
                RejectionReason = r.RejectionReason,
                CreatedAt = r.CreatedAt,
                SpecialCategories = specialCategories,
                AcademicStatus = r.Student?.AcademicStatus ?? string.Empty
            };
        }
    }
}
