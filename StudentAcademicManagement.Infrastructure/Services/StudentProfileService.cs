using Microsoft.EntityFrameworkCore;
using StudentAcademicManagement.Application.DTOs.Students;
using StudentAcademicManagement.Application.Interfaces;
using StudentAcademicManagement.Domain.Entities;
using StudentAcademicManagement.Infrastructure.Persistence;

namespace StudentAcademicManagement.Infrastructure.Services
{
    public class StudentProfileService : IStudentProfileService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorageService;

        public StudentProfileService(ApplicationDbContext context, IFileStorageService fileStorageService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
        }

        public async Task<StudentProfileResponse> GetProfileByStudentIdAsync(int schoolId, int studentId)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Profile)
                .FirstOrDefaultAsync(s => s.Id == studentId && s.SchoolId == schoolId);

            if (student == null) throw new KeyNotFoundException("Không tìm thấy sinh viên trong trường này.");

            return MapToResponse(student);
        }

        public async Task<StudentProfileResponse> UpdateProfileAsync(int schoolId, int studentId, UpdateStudentProfileRequest request)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Profile)
                .Include(s => s.Identity) // RẤT QUAN TRỌNG: Include Identity để kiểm tra cờ IsLocked
                .FirstOrDefaultAsync(s => s.Id == studentId && s.SchoolId == schoolId);

            if (student == null) throw new KeyNotFoundException("Không tìm thấy sinh viên.");

            // Khởi tạo Profile nếu chưa có
            if (student.Profile == null)
            {
                student.Profile = new StudentProfile { StudentId = student.Id };
                _context.StudentProfiles.Add(student.Profile);
            }

            // Xử lý Avatar (Luôn cho phép đổi Avatar bất kể bị khóa hay chưa)
            if (request.Avatar != null)
            {
                student.Profile.AvatarUrl = await _fileStorageService.SaveFileAsync(request.Avatar, "avatars");
            }

            // ===============================================================
            // LOGIC BẢO VỆ DỮ LIỆU TỪ CCCD
            // ===============================================================
            if (student.Identity != null && student.Identity.IsLocked)
            {
                // Nếu dữ liệu đã bị khóa bởi CCCD, chỉ cho phép cập nhật các trường không cốt lõi
                // BỎ QUA không ghi đè FullName, DateOfBirth, Gender.

                student.Profile.Ethnicity = request.Ethnicity;
                student.Profile.Nationality = request.Nationality;
                student.Profile.PlaceOfBirth = request.PlaceOfBirth;
                student.Profile.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Dừng lại và trả về ngay để bảo vệ các field cốt lõi
                return MapToResponse(student);
            }

            // ===============================================================
            // NẾU CHƯA BỊ KHÓA HOẶC CHƯA CÓ CCCD
            // ===============================================================
            // Cho phép cập nhật toàn bộ thông tin
            student.Profile.FullName = request.FullName;
            student.Profile.DateOfBirth = request.DateOfBirth;
            student.Profile.Gender = request.Gender;

            student.Profile.Ethnicity = request.Ethnicity;
            student.Profile.Nationality = request.Nationality;
            student.Profile.PlaceOfBirth = request.PlaceOfBirth;
            student.Profile.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToResponse(student);
        }

        private StudentProfileResponse MapToResponse(Student student)
        {
            return new StudentProfileResponse
            {
                Id = student.Profile?.Id ?? 0,
                StudentId = student.Id,
                StudentCode = student.StudentCode,
                Email = student.User.Email,
                FullName = student.Profile?.FullName ?? string.Empty,
                DateOfBirth = student.Profile?.DateOfBirth,
                Gender = student.Profile?.Gender,
                Ethnicity = student.Profile?.Ethnicity,
                Nationality = student.Profile?.Nationality,
                PlaceOfBirth = student.Profile?.PlaceOfBirth,
                AvatarUrl = student.Profile?.AvatarUrl,
                FacultyName = student.FacultyName,
                MajorName = student.MajorName,
                ClassName = student.ClassName,
                AcademicStatus = student.AcademicStatus
            };
        }
    }
}