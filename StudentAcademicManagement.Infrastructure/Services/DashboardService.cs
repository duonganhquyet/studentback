using Microsoft.EntityFrameworkCore;
using StudentAcademicManagement.Application.DTOs.Dashboards;
using StudentAcademicManagement.Application.Interfaces;
using StudentAcademicManagement.Infrastructure.Persistence;

namespace StudentAcademicManagement.Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SuperAdminDashboardResponse> GetSuperAdminDashboardAsync()
        {
            var totalSchools = await _context.Schools.CountAsync();
            var activeSchools = await _context.Schools.CountAsync(s => s.IsActive);
            var totalSchoolAdmins = await _context.Users.CountAsync(u => u.RoleId == 2); // Role 2 là SchoolAdmin
            var totalSystemUsers = await _context.Users.CountAsync();

            return new SuperAdminDashboardResponse
            {
                TotalSchools = totalSchools,
                ActiveSchools = activeSchools,
                TotalSchoolAdmins = totalSchoolAdmins,
                TotalSystemUsers = totalSystemUsers
            };
        }

        public async Task<SchoolAdminDashboardResponse> GetSchoolAdminDashboardAsync(int schoolId)
        {
            var queryStudents = _context.Students.Where(s => s.SchoolId == schoolId);

            var totalStudents = await queryStudents.CountAsync();
            var studyingStudents = await queryStudents.CountAsync(s => s.AcademicStatus == "Studying");
            var quitOrSuspended = await queryStudents.CountAsync(s => s.AcademicStatus == "Quit" || s.AcademicStatus == "Suspended");

            var pendingCccd = await _context.StudentEditRequests
                .Include(r => r.Student)
                .CountAsync(r => r.Student.SchoolId == schoolId && r.Status == "Pending");

            var pendingDocs = await _context.StudentDocuments
                .Include(d => d.Student)
                .CountAsync(d => d.Student.SchoolId == schoolId && d.Status == "Pending");

            var pendingSpecials = await _context.StudentSpecialCategories
                .Include(sc => sc.Student)
                .CountAsync(sc => sc.Student.SchoolId == schoolId && sc.Status == "Pending");

            return new SchoolAdminDashboardResponse
            {
                TotalStudents = totalStudents,
                StudyingStudents = studyingStudents,
                QuitOrSuspendedStudents = quitOrSuspended,
                PendingCccdRequests = pendingCccd,
                PendingDocuments = pendingDocs,
                PendingSpecialCategories = pendingSpecials
            };
        }

        public async Task<StudentDashboardResponse> GetStudentDashboardAsync(int userId)
        {
            var student = await _context.Students
                .Include(s => s.Profile)
                .Include(s => s.Identity)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null) throw new UnauthorizedAccessException("Không tìm thấy sinh viên.");

            var uploadedDocs = await _context.StudentDocuments.CountAsync(d => d.StudentId == student.Id);
            var approvedSpecials = await _context.StudentSpecialCategories.CountAsync(sc => sc.StudentId == student.Id && sc.Status == "Approved");
            var pendingDocs = await _context.StudentDocuments.CountAsync(d => d.StudentId == student.Id && d.Status == "Pending");
            var pendingSpecials = await _context.StudentSpecialCategories.CountAsync(sc => sc.StudentId == student.Id && sc.Status == "Pending");

            return new StudentDashboardResponse
            {
                StudentCode = student.StudentCode,
                FullName = student.Identity?.FullName ?? student.Profile?.FullName ?? student.StudentCode,
                AcademicStatus = student.AcademicStatus,
                CccdVerificationStatus = student.Identity?.VerificationStatus ?? "Unverified",
                IsIdentityLocked = student.Identity?.IsLocked ?? false,
                UploadedDocumentsCount = uploadedDocs,
                ApprovedSpecialCategoriesCount = approvedSpecials,
                TotalPendingDocuments = pendingDocs,
                TotalPendingSpecialCategories = pendingSpecials
            };
        }
    }
}