using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StudentAcademicManagement.Application.DTOs.Schools;
using StudentAcademicManagement.Application.Interfaces;
using StudentAcademicManagement.Domain.Entities;
using StudentAcademicManagement.Infrastructure.Persistence;

namespace StudentAcademicManagement.Infrastructure.Services
{
    public class SchoolService : ISchoolService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorageService;
        private readonly IAuditLogService _auditLogService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SchoolService(
            ApplicationDbContext context,
            IFileStorageService fileStorageService,
            IAuditLogService auditLogService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _fileStorageService = fileStorageService;
            _auditLogService = auditLogService;
            _httpContextAccessor = httpContextAccessor;
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
            return int.TryParse(userIdClaim, out int uid) ? uid : null;
        }

        public async Task<SchoolResponse> CreateSchoolAsync(CreateSchoolRequest request)
        {
            var exists = await _context.Schools.AnyAsync(s => s.SchoolCode == request.SchoolCode);
            if (exists) throw new InvalidOperationException("Mã trường đã tồn tại trong hệ thống.");

            string? logoUrl = null;
            if (request.Logo != null) logoUrl = await _fileStorageService.SaveFileAsync(request.Logo, "logos");

            var school = new School
            {
                SchoolCode = request.SchoolCode,
                SchoolName = request.SchoolName,
                ShortName = request.ShortName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                Website = request.Website,
                Description = request.Description,
                LogoUrl = logoUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Schools.Add(school);
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                userId: GetCurrentUserId(),
                schoolId: school.Id,
                action: "Tạo Trường Học Mới",
                entityName: "School",
                entityId: school.Id.ToString(),
                newValue: $"Tạo trường: {school.SchoolName} ({school.SchoolCode})"
            );

            return MapToResponse(school);
        }

        public async Task<IEnumerable<SchoolResponse>> GetAllSchoolsAsync()
        {
            var schools = await _context.Schools.OrderByDescending(s => s.CreatedAt).ToListAsync();
            return schools.Select(MapToResponse).ToList();
        }

        public async Task<SchoolResponse> GetSchoolByIdAsync(int id)
        {
            var school = await _context.Schools.FindAsync(id);
            if (school == null) throw new KeyNotFoundException("Không tìm thấy trường học.");
            return MapToResponse(school);
        }

        public async Task<SchoolResponse> UpdateSchoolAsync(int id, UpdateSchoolRequest request)
        {
            var school = await _context.Schools.FindAsync(id);
            if (school == null) throw new KeyNotFoundException("Không tìm thấy trường học.");

            if (request.Logo != null)
            {
                var newLogoUrl = await _fileStorageService.SaveFileAsync(request.Logo, "logos");
                school.LogoUrl = newLogoUrl;
            }

            string oldName = school.SchoolName;

            school.SchoolName = request.SchoolName;
            school.ShortName = request.ShortName;
            school.Email = request.Email;
            school.PhoneNumber = request.PhoneNumber;
            school.Address = request.Address;
            school.Website = request.Website;
            school.Description = request.Description;
            school.UpdatedAt = DateTime.UtcNow;

            _context.Schools.Update(school);
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                userId: GetCurrentUserId(),
                schoolId: school.Id,
                action: "Cập Nhật Thông Tin Trường",
                entityName: "School",
                entityId: school.Id.ToString(),
                oldValue: oldName,
                newValue: school.SchoolName
            );

            return MapToResponse(school);
        }

        public async Task ChangeSchoolStatusAsync(int id, bool isActive)
        {
            var school = await _context.Schools.FindAsync(id);
            if (school == null) throw new KeyNotFoundException("Không tìm thấy trường học.");

            bool oldStatus = school.IsActive;
            school.IsActive = isActive;
            school.UpdatedAt = DateTime.UtcNow;

            _context.Schools.Update(school);
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                userId: GetCurrentUserId(),
                schoolId: school.Id,
                action: isActive ? "Kích Hoạt Trường Học" : "Khóa Trường Học",
                entityName: "School",
                entityId: school.Id.ToString(),
                oldValue: oldStatus ? "Active" : "Locked",
                newValue: isActive ? "Active" : "Locked"
            );
        }

        private SchoolResponse MapToResponse(School school)
        {
            return new SchoolResponse
            {
                Id = school.Id,
                SchoolCode = school.SchoolCode,
                SchoolName = school.SchoolName,
                ShortName = school.ShortName,
                Email = school.Email,
                PhoneNumber = school.PhoneNumber,
                Address = school.Address,
                LogoUrl = school.LogoUrl,
                Website = school.Website,
                Description = school.Description,
                IsActive = school.IsActive,
                CreatedAt = school.CreatedAt
            };
        }
    }
}