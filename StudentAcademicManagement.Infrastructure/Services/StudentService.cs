using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using StudentAcademicManagement.Application.DTOs.Common;
using StudentAcademicManagement.Application.DTOs.Students;
using StudentAcademicManagement.Application.Interfaces;
using StudentAcademicManagement.Domain.Entities;
using StudentAcademicManagement.Infrastructure.Persistence;

namespace StudentAcademicManagement.Infrastructure.Services
{
    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public StudentService(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        private static string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            var randomBytes = new byte[6];
            RandomNumberGenerator.Fill(randomBytes);
            var result = new char[6];
            for (int i = 0; i < 6; i++)
            {
                result[i] = chars[randomBytes[i] % chars.Length];
            }
            return $"Sv@{new string(result)}!";
        }

        public async Task<StudentResponse> CreateStudentAsync(int schoolId, CreateStudentRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                throw new InvalidOperationException("Email này đã được sử dụng.");

            if (await _context.Students.AnyAsync(s => s.SchoolId == schoolId && s.StudentCode == request.StudentCode))
                throw new InvalidOperationException($"Mã sinh viên {request.StudentCode} đã tồn tại trong trường này.");

            var studentRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Student")
                              ?? throw new InvalidOperationException("Role Student chưa được cấu hình.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = new User
                {
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.TemporaryPassword),
                    RoleId = studentRole.Id,
                    SchoolId = schoolId,
                    IsFirstLogin = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var student = new Student
                {
                    StudentCode = request.StudentCode.Trim(),
                    UserId = user.Id,
                    SchoolId = schoolId,
                    FacultyName = request.FacultyName.Trim(),
                    MajorName = request.MajorName.Trim(),
                    ClassName = request.ClassName.Trim(),
                    AcademicStatus = "Studying",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                var school = await _context.Schools.FindAsync(schoolId);
                string schoolName = school?.SchoolName ?? "Trường Học";

                string emailBody = $@"
                    Xin chào sinh viên,
                    Tài khoản học vụ của bạn tại {schoolName} đã được khởi tạo thành công:
                    - Tên đăng nhập (MSSV): {request.StudentCode}
                    - Email liên hệ: {request.Email}
                    - Khoa: {request.FacultyName}
                    - Ngành: {request.MajorName}
                    - Lớp quản lý: {request.ClassName}
                    - Mật khẩu tạm thời: {request.TemporaryPassword}

                    Vui lòng đăng nhập hệ thống và đổi mật khẩu ở lần đăng nhập đầu tiên!
                ";
                await _emailService.SendEmailAsync(request.Email, $"Thông tin Tài khoản Sinh viên {request.StudentCode}", emailBody);

                return new StudentResponse
                {
                    Id = student.Id,
                    UserId = user.Id,
                    StudentCode = student.StudentCode,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    FacultyName = student.FacultyName,
                    MajorName = student.MajorName,
                    ClassName = student.ClassName,
                    CreatedAt = student.CreatedAt
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<BatchImportResult> BatchImportStudentsAsync(int schoolId, List<BatchImportStudentItem> items)
        {
            var result = new BatchImportResult();
            var studentRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Student");
            if (studentRole == null)
            {
                result.ErrorMessages.Add("Cấu hình Role Student chưa tồn tại trong hệ thống.");
                return result;
            }

            var school = await _context.Schools.FindAsync(schoolId);
            string schoolName = school?.SchoolName ?? "Trường Học";

            foreach (var item in items)
            {
                var code = (item.StudentCode ?? string.Empty).Trim();
                var email = (item.Email ?? string.Empty).Trim();
                var faculty = (item.FacultyName ?? string.Empty).Trim();
                var major = (item.MajorName ?? string.Empty).Trim();
                var className = (item.ClassName ?? string.Empty).Trim();

                if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(email))
                {
                    result.FailureCount++;
                    result.ErrorMessages.Add($"Dòng bị bỏ qua: MSSV hoặc Email không được để trống.");
                    continue;
                }

                if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower()))
                {
                    result.FailureCount++;
                    result.ErrorMessages.Add($"MSSV {code} ({email}): Email này đã tồn tại trong hệ thống.");
                    continue;
                }

                if (await _context.Students.AnyAsync(s => s.SchoolId == schoolId && s.StudentCode.ToLower() == code.ToLower()))
                {
                    result.FailureCount++;
                    result.ErrorMessages.Add($"MSSV {code}: Mã sinh viên đã tồn tại trong trường.");
                    continue;
                }

                var tempPassword = GenerateRandomPassword();

                try
                {
                    var user = new User
                    {
                        Email = email,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword),
                        RoleId = studentRole.Id,
                        SchoolId = schoolId,
                        IsFirstLogin = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    var student = new Student
                    {
                        StudentCode = code,
                        UserId = user.Id,
                        SchoolId = schoolId,
                        FacultyName = faculty,
                        MajorName = major,
                        ClassName = className,
                        AcademicStatus = "Studying",
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Students.Add(student);
                    await _context.SaveChangesAsync();

                    string emailBody = $@"
                        Xin chào sinh viên {code},
                        Tài khoản sinh viên tại {schoolName} đã được cấp tự động:
                        -----------------------------------------
                        - Tên đăng nhập (MSSV): {code}
                        - Email: {email}
                        - Khoa: {faculty}
                        - Ngành: {major}
                        - Lớp: {className}
                        - Mật khẩu tạm thời: {tempPassword}
                        -----------------------------------------
                        Vui lòng đăng nhập hệ thống bằng MSSV/Email và mật khẩu tạm thời ở trên, sau đó tiến hành đổi mật khẩu.
                    ";

                    await _emailService.SendEmailAsync(email, $"Tài khoản Sinh viên {code} - {schoolName}", emailBody);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.FailureCount++;
                    result.ErrorMessages.Add($"MSSV {code}: Lỗi khởi tạo - {ex.Message}");
                }
            }

            return result;
        }

        public async Task<PagedResult<StudentListResponse>> GetStudentsAsync(int schoolId, StudentFilterRequest request)
        {
            var query = _context.Students
                .Include(s => s.User)
                .Include(s => s.Profile)
                .Where(s => s.SchoolId == schoolId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower().Trim();
                query = query.Where(s =>
                    (s.StudentCode != null && s.StudentCode.ToLower().Contains(searchTerm)) ||
                    (s.User != null && s.User.Email != null && s.User.Email.ToLower().Contains(searchTerm)) ||
                    (s.FacultyName != null && s.FacultyName.ToLower().Contains(searchTerm)) ||
                    (s.MajorName != null && s.MajorName.ToLower().Contains(searchTerm)) ||
                    (s.ClassName != null && s.ClassName.ToLower().Contains(searchTerm)) ||
                    (s.Profile != null && s.Profile.FullName != null && s.Profile.FullName.ToLower().Contains(searchTerm)));
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(s => s.User.IsActive == request.IsActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.FacultyName))
            {
                var facultyTerm = request.FacultyName.ToLower().Trim();
                query = query.Where(s => s.FacultyName != null && s.FacultyName.ToLower().Contains(facultyTerm));
            }

            if (!string.IsNullOrWhiteSpace(request.MajorName))
            {
                var majorTerm = request.MajorName.ToLower().Trim();
                query = query.Where(s => s.MajorName != null && s.MajorName.ToLower().Contains(majorTerm));
            }

            if (!string.IsNullOrWhiteSpace(request.ClassName))
            {
                var classTerm = request.ClassName.ToLower().Trim();
                query = query.Where(s => s.ClassName != null && s.ClassName.ToLower().Contains(classTerm));
            }

            var totalCount = await query.CountAsync();

            var students = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(s => new StudentListResponse
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    StudentCode = s.StudentCode,
                    Email = s.User.Email,
                    IsActive = s.User.IsActive,
                    IsFirstLogin = s.User.IsFirstLogin,
                    AcademicStatus = s.AcademicStatus,
                    FullName = s.Profile != null ? s.Profile.FullName : string.Empty,
                    AvatarUrl = s.Profile != null ? s.Profile.AvatarUrl : null,
                    FacultyName = s.FacultyName,
                    MajorName = s.MajorName,
                    ClassName = s.ClassName,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            return new PagedResult<StudentListResponse>
            {
                Items = students,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task ChangeStudentStatusAsync(int schoolId, int studentId, int adminUserId, ChangeStudentStatusRequest request)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == studentId && s.SchoolId == schoolId);

            if (student == null) throw new KeyNotFoundException("Không tìm thấy sinh viên.");

            var oldStatus = student.AcademicStatus;
            if (oldStatus == request.NewStatus) throw new InvalidOperationException("Trạng thái mới không được trùng trạng thái cũ.");

            student.AcademicStatus = request.NewStatus;
            student.UpdatedAt = DateTime.UtcNow;

            if (request.NewStatus == "Quit" || request.NewStatus == "Suspended")
            {
                student.User.IsActive = false;
            }
            else
            {
                student.User.IsActive = true;
            }

            var auditLog = new AuditLog
            {
                UserId = adminUserId,
                SchoolId = schoolId,
                Action = "Change Academic Status",
                EntityName = "Student",
                EntityId = student.Id.ToString(),
                OldValue = oldStatus,
                NewValue = $"{request.NewStatus} (Lý do: {request.Reason})",
                CreatedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStudentAcademicInfoAsync(int schoolId, int studentId, UpdateStudentAcademicRequest request)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == studentId && s.SchoolId == schoolId);
            if (student == null) throw new KeyNotFoundException("Không tìm thấy sinh viên trong hệ thống.");

            student.FacultyName = (request.FacultyName ?? string.Empty).Trim();
            student.MajorName = (request.MajorName ?? string.Empty).Trim();
            student.ClassName = (request.ClassName ?? string.Empty).Trim();
            student.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}