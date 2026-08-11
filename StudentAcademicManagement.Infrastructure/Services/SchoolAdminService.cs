using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StudentAcademicManagement.Application.DTOs.SchoolAdmins;
using StudentAcademicManagement.Application.Interfaces;
using StudentAcademicManagement.Domain.Entities;
using StudentAcademicManagement.Infrastructure.Persistence;

namespace StudentAcademicManagement.Infrastructure.Services
{
    public class SchoolAdminService : ISchoolAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IAuditLogService _auditLogService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SchoolAdminService(
            ApplicationDbContext context,
            IEmailService emailService,
            IAuditLogService auditLogService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _emailService = emailService;
            _auditLogService = auditLogService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<SchoolAdminResponse> CreateSchoolAdminAsync(CreateSchoolAdminRequest request)
        {
            // 1. Kiểm tra Email đã tồn tại chưa
            var emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email);
            if (emailExists)
                throw new InvalidOperationException("Email này đã được sử dụng trong hệ thống.");

            // 2. Kiểm tra Trường học (SchoolId) có tồn tại không
            var school = await _context.Schools.FindAsync(request.SchoolId);
            if (school == null)
                throw new KeyNotFoundException("Không tìm thấy trường học được chỉ định.");

            // 3. Lấy Role SchoolAdmin (Id = 2 theo seed data)
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "SchoolAdmin");
            if (role == null)
                throw new InvalidOperationException("Role SchoolAdmin chưa được cấu hình.");

            // 4. Tạo User
            var adminUser = new User
            {
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.TemporaryPassword),
                RoleId = role.Id,
                SchoolId = request.SchoolId,
                IsFirstLogin = true, // Bắt buộc đổi MK lần đầu
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();

            // 5. Ghi Nhật ký Hệ thống (Audit Log)
            int? currentUserId = null;
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
            if (int.TryParse(userIdClaim, out int uid))
            {
                currentUserId = uid;
            }

            await _auditLogService.LogAsync(
                userId: currentUserId,
                schoolId: request.SchoolId,
                action: "Tạo Quản trị viên Trường",
                entityName: "User",
                entityId: adminUser.Id.ToString(),
                newValue: $"Tạo tài khoản SchoolAdmin {request.Email} cho trường {school.SchoolName}"
            );

            // 6. Gửi Email thông báo tài khoản
            string emailBody = $@"
                Xin chào,
                Tài khoản School Admin của bạn cho trường {school.SchoolName} đã được khởi tạo.
                - Email đăng nhập: {request.Email}
                - Mật khẩu tạm thời: {request.TemporaryPassword}
                Vui lòng đăng nhập và đổi mật khẩu trong lần đầu tiên truy cập.
            ";
            await _emailService.SendEmailAsync(request.Email, "Tài khoản School Admin", emailBody);

            return new SchoolAdminResponse
            {
                Id = adminUser.Id,
                Email = adminUser.Email,
                SchoolId = adminUser.SchoolId.Value,
                SchoolName = school.SchoolName,
                IsActive = adminUser.IsActive,
                CreatedAt = adminUser.CreatedAt
            };
        }

        public async Task<IEnumerable<SchoolAdminResponse>> GetAllSchoolAdminsAsync()
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "SchoolAdmin");
            if (role == null) return new List<SchoolAdminResponse>();

            var admins = await _context.Users
                .Where(u => u.RoleId == role.Id)
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new SchoolAdminResponse
                {
                    Id = u.Id,
                    Email = u.Email,
                    SchoolId = u.SchoolId ?? 0,
                    SchoolName = _context.Schools.Where(s => s.Id == u.SchoolId).Select(s => s.SchoolName).FirstOrDefault() ?? "Unknown",
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return admins;
        }
    }
}