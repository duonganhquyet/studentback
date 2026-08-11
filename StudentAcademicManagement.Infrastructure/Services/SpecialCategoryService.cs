using Microsoft.EntityFrameworkCore;
using StudentAcademicManagement.Application.DTOs.SpecialCategories;
using StudentAcademicManagement.Application.Interfaces;
using StudentAcademicManagement.Domain.Entities;
using StudentAcademicManagement.Infrastructure.Persistence;

namespace StudentAcademicManagement.Infrastructure.Services
{
    public class SpecialCategoryService : ISpecialCategoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorageService;

        public SpecialCategoryService(ApplicationDbContext context, IFileStorageService fileStorageService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
        }

        // ================= DANH MỤC (SCHOOL ADMIN) =================
        public async Task<IEnumerable<SpecialCategoryResponse>> GetCategoriesAsync(int schoolId)
        {
            var list = await _context.SpecialCategories
                .Where(c => c.SchoolId == schoolId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return list.Select(c => new SpecialCategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive
            });
        }

        public async Task<SpecialCategoryResponse> CreateCategoryAsync(int schoolId, CreateSpecialCategoryRequest request)
        {
            var category = new SpecialCategory
            {
                SchoolId = schoolId,
                Name = request.Name,
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.SpecialCategories.Add(category);
            await _context.SaveChangesAsync();

            return new SpecialCategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive
            };
        }

        public async Task ToggleCategoryStatusAsync(int schoolId, int id)
        {
            var category = await _context.SpecialCategories.FirstOrDefaultAsync(c => c.Id == id && c.SchoolId == schoolId);
            if (category == null) throw new KeyNotFoundException("Không tìm thấy danh mục.");

            category.IsActive = !category.IsActive;
            category.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        // ================= SINH VIÊN ĐĂNG KÝ =================
        public async Task<StudentSpecialCategoryResponse> RegisterSpecialCategoryAsync(int userId, RegisterSpecialCategoryRequest request)
        {
            var student = await _context.Students.Include(s => s.Profile).FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) throw new UnauthorizedAccessException();

            var category = await _context.SpecialCategories.FirstOrDefaultAsync(c => c.Id == request.SpecialCategoryId && c.SchoolId == student.SchoolId && c.IsActive);
            if (category == null) throw new InvalidOperationException("Đối tượng này không tồn tại hoặc đã bị khóa.");

            // Kiểm tra xem đã đăng ký đối tượng này chưa (Pending hoặc Approved)
            var existing = await _context.StudentSpecialCategories
                .AnyAsync(s => s.StudentId == student.Id && s.SpecialCategoryId == category.Id && s.Status != "Rejected");

            if (existing) throw new InvalidOperationException("Bạn đã đăng ký đối tượng này rồi.");

            var fileUrl = await _fileStorageService.SaveFileAsync(request.ProofFile, "special_categories");

            var reg = new StudentSpecialCategory
            {
                StudentId = student.Id,
                SpecialCategoryId = category.Id,
                ProofFileUrl = fileUrl,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.StudentSpecialCategories.Add(reg);
            await _context.SaveChangesAsync();

            return new StudentSpecialCategoryResponse
            {
                Id = reg.Id,
                StudentId = student.Id,
                StudentCode = student.StudentCode,
                FullName = student.Profile?.FullName ?? "",
                SpecialCategoryId = category.Id,
                CategoryName = category.Name,
                ProofFileUrl = fileUrl,
                Status = reg.Status,
                CreatedAt = reg.CreatedAt
            };
        }

        public async Task<IEnumerable<StudentSpecialCategoryResponse>> GetMyRegistrationsAsync(int userId)
        {
            var student = await _context.Students.Include(s => s.Profile).FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) throw new UnauthorizedAccessException();

            var regs = await _context.StudentSpecialCategories
                .Include(r => r.SpecialCategory)
                .Where(r => r.StudentId == student.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return regs.Select(r => new StudentSpecialCategoryResponse
            {
                Id = r.Id,
                CategoryName = r.SpecialCategory.Name,
                ProofFileUrl = r.ProofFileUrl,
                Status = r.Status,
                RejectionReason = r.RejectionReason,
                CreatedAt = r.CreatedAt
            });
        }

        // ================= ADMIN DUYỆT =================
        public async Task<IEnumerable<StudentSpecialCategoryResponse>> GetPendingRegistrationsAsync(int schoolId)
        {
            var regs = await _context.StudentSpecialCategories
                .Include(r => r.SpecialCategory)
                .Include(r => r.Student)
                    .ThenInclude(s => s.Profile)
                .Where(r => r.Student.SchoolId == schoolId && r.Status == "Pending")
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return regs.Select(r => new StudentSpecialCategoryResponse
            {
                Id = r.Id,
                StudentId = r.StudentId,
                StudentCode = r.Student.StudentCode,
                FullName = r.Student.Profile?.FullName ?? "",
                CategoryName = r.SpecialCategory.Name,
                ProofFileUrl = r.ProofFileUrl,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            });
        }

        public async Task ReviewRegistrationAsync(int schoolId, int registrationId, ReviewSpecialCategoryRequest request)
        {
            var reg = await _context.StudentSpecialCategories
                .Include(r => r.Student)
                .FirstOrDefaultAsync(r => r.Id == registrationId);

            if (reg == null) throw new KeyNotFoundException("Không tìm thấy đơn đăng ký.");
            if (reg.Student.SchoolId != schoolId) throw new UnauthorizedAccessException("Không có quyền.");
            if (reg.Status != "Pending") throw new InvalidOperationException("Đơn này đã được xử lý.");

            reg.Status = request.Status;
            reg.RejectionReason = request.Status == "Rejected" ? request.RejectionReason : null;
            reg.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}