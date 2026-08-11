using Microsoft.EntityFrameworkCore;
using StudentAcademicManagement.Application.DTOs.Students;
using StudentAcademicManagement.Application.Interfaces;
using StudentAcademicManagement.Domain.Entities;
using StudentAcademicManagement.Infrastructure.Persistence;
using System.Text.Json;

namespace StudentAcademicManagement.Infrastructure.Services
{
    public class StudentIdentityService : IStudentIdentityService
    {
        private readonly ApplicationDbContext _context;
        private readonly IOcrService _ocrService;
        private readonly IFileStorageService _fileStorageService;

        public StudentIdentityService(ApplicationDbContext context, IOcrService ocrService, IFileStorageService fileStorageService)
        {
            _context = context;
            _ocrService = ocrService;
            _fileStorageService = fileStorageService;
        }

        // =========================================================================
        // KHỐI NGHIỆP VỤ SINH VIÊN TỰ XỬ LÝ CCCD & OCR
        // =========================================================================

        public async Task<StudentIdentityResponse?> GetIdentityAsync(int userId)
        {
            var student = await _context.Students
                .Include(s => s.Profile)
                .Include(s => s.Identity)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null) return null;

            return new StudentIdentityResponse
            {
                StudentCode = student.StudentCode,
                IdNumber = student.Identity?.IdNumber,
                FullName = student.Identity?.FullName ?? student.Profile?.FullName,
                DateOfBirth = student.Identity?.DateOfBirth ?? student.Profile?.DateOfBirth,
                Gender = student.Identity?.Gender ?? student.Profile?.Gender,
                PlaceOfOrigin = student.Identity?.PlaceOfOrigin,
                PlaceOfResidence = student.Identity?.PlaceOfResidence,
                IssueDate = student.Identity?.IssueDate,
                IssuePlace = student.Identity?.IssuePlace,
                VerificationStatus = student.Identity?.VerificationStatus ?? "Unverified",
                IsLocked = student.Identity?.IsLocked ?? false,
                FrontImageUrl = student.Identity?.FrontImageUrl,
                BackImageUrl = student.Identity?.BackImageUrl
            };
        }

        public async Task<CccdComparisonResponse> UploadAndProcessCccdAsync(int userId, UploadCccdRequest request)
        {
            var student = await _context.Students
                .Include(s => s.Profile)
                .Include(s => s.Identity)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null) throw new UnauthorizedAccessException("Không tìm thấy thông tin sinh viên hợp lệ.");
            if (student.Identity?.IsLocked == true) throw new InvalidOperationException("CCCD đã được xác thực và khóa. Không thể tải lên lại.");

            // 1. Lưu file ảnh vật lý trên đĩa
            var frontUrl = await _fileStorageService.SaveFileAsync(request.FrontImage, "cccd");
            var backUrl = await _fileStorageService.SaveFileAsync(request.BackImage, "cccd");

            // 2. Chạy OCR (Trích xuất dữ liệu từ mặt trước)
            var ocrData = await _ocrService.ExtractCccdDataAsync(request.FrontImage);

            // 3. Cập nhật nháp vào Database
            if (student.Identity == null)
            {
                student.Identity = new StudentIdentity { StudentId = student.Id };
                _context.StudentIdentities.Add(student.Identity);
            }

            student.Identity.FrontImageUrl = frontUrl;
            student.Identity.BackImageUrl = backUrl;
            student.Identity.VerificationStatus = "Pending";

            // 3.5 Xóa tài liệu CCCD cũ nếu có để tránh trùng lặp
            var existingDocs = await _context.StudentDocuments
                .Where(d => d.StudentId == student.Id && d.DocumentType == "Identity")
                .ToListAsync();
            if (existingDocs.Any())
            {
                _context.StudentDocuments.RemoveRange(existingDocs);
            }

            _context.StudentDocuments.Add(new StudentDocument
            {
                StudentId = student.Id,
                DocumentName = "Ảnh CCCD (Mặt trước)",
                DocumentType = "Identity",
                FileUrl = frontUrl,
                Status = "Approved",
                CreatedAt = DateTime.UtcNow
            });

            _context.StudentDocuments.Add(new StudentDocument
            {
                StudentId = student.Id,
                DocumentName = "Ảnh CCCD (Mặt sau)",
                DocumentType = "Identity",
                FileUrl = backUrl,
                Status = "Approved",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            // 4. Trả về kết quả so sánh
            return new CccdComparisonResponse
            {
                OcrData = ocrData,
                FrontImageUrl = frontUrl,
                BackImageUrl = backUrl,
                CurrentProfile = new StudentProfileResponse
                {
                    FullName = student.Profile?.FullName ?? "",
                    DateOfBirth = student.Profile?.DateOfBirth,
                    Gender = student.Profile?.Gender,
                    PlaceOfBirth = student.Profile?.PlaceOfBirth
                }
            };
        }

        public async Task ConfirmAndLockIdentityAsync(int userId, ConfirmCccdRequest request)
        {
            var student = await _context.Students
                .Include(s => s.Profile)
                .Include(s => s.Identity)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null) throw new UnauthorizedAccessException("Dữ liệu sinh viên không hợp lệ.");

            if (student.Identity == null)
            {
                student.Identity = new StudentIdentity { StudentId = student.Id };
                _context.StudentIdentities.Add(student.Identity);
            }

            if (student.Identity.IsLocked) throw new InvalidOperationException("Hồ sơ này đã bị khóa dữ liệu từ trước.");

            // 1. Cập nhật Identity chuẩn
            student.Identity.IdNumber = request.IdNumber;
            student.Identity.FullName = request.FullName;
            student.Identity.DateOfBirth = request.Dob;
            student.Identity.Gender = request.Gender;
            student.Identity.PlaceOfResidence = request.Address;
            student.Identity.IssueDate = request.IssueDate;
            student.Identity.IssuePlace = request.IssuePlace;
            student.Identity.VerificationStatus = "Verified";

            // 1.5 Lưu danh sách gia đình
            if (request.FamilyMembers != null && request.FamilyMembers.Any())
            {
                foreach (var m in request.FamilyMembers)
                {
                    _context.FamilyMembers.Add(new StudentFamilyMember
                    {
                        StudentId = student.Id,
                        FullName = m.FullName,
                        Relationship = m.Relationship,
                        Nationality = m.Nationality,
                        BirthYear = m.BirthYear,
                        Job = m.Job,
                        Position = m.Position,
                        Company = m.Company,
                        Ethnicity = m.Ethnicity,
                        Religion = m.Religion,
                        PhoneNumber = m.Phone,
                        PermanentAddress = m.PermanentAddress,
                        CurrentAddress = m.CurrentAddress,
                        IsEmergencyContact = m.IsEmergencyContact,
                        IsAlumni = m.IsAlumni
                    });
                }
            }

            // Lệnh Khóa bảo vệ
            student.Identity.IsLocked = true;

            // 2. Đồng bộ dữ liệu sang Profile của sinh viên
            if (student.Profile == null)
            {
                student.Profile = new StudentProfile { StudentId = student.Id };
                _context.StudentProfiles.Add(student.Profile);
            }
            student.Profile.FullName = request.FullName;
            student.Profile.DateOfBirth = request.Dob;
            student.Profile.Gender = request.Gender;

            await _context.SaveChangesAsync();
        }

        // =========================================================================
        // KHỐI NGHIỆP VỤ YÊU CẦU CHỈNH SỬA DỮ LIỆU ĐÃ KHÓA (EDIT REQUEST)
        // =========================================================================

        public async Task<EditRequestResponse> CreateEditRequestAsync(int userId, CreateEditRequest request)
        {
            var student = await _context.Students
                .Include(s => s.Identity)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null) throw new UnauthorizedAccessException("Tài khoản không hợp lệ.");

            // Chỉ sinh viên đã khóa dữ liệu mới cần tạo Edit Request
            if (student.Identity == null || !student.Identity.IsLocked)
                throw new InvalidOperationException("Dữ liệu của bạn chưa bị khóa, không cần gửi yêu cầu.");

            // Kiểm tra xem có Request nào đang Pending không
            var hasPending = await _context.StudentEditRequests
                .AnyAsync(r => r.StudentId == student.Id && r.Status == "Pending");
            if (hasPending)
                throw new InvalidOperationException("Bạn đang có một yêu cầu chờ duyệt, vui lòng không gửi thêm.");

            var editReq = new StudentEditRequest
            {
                StudentId = student.Id,
                RequestedFullName = request.RequestedFullName,
                RequestedDateOfBirth = request.RequestedDateOfBirth,
                RequestedGender = request.RequestedGender,
                Reason = request.Reason,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.StudentEditRequests.Add(editReq);
            await _context.SaveChangesAsync();

            return MapToEditResponse(editReq, student.StudentCode);
        }

        public async Task<EditRequestResponse?> GetMyPendingEditRequestAsync(int userId)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) return null;

            var req = await _context.StudentEditRequests
                .Where(r => r.StudentId == student.Id && r.Status == "Pending")
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            return req != null ? MapToEditResponse(req, student.StudentCode) : null;
        }

        public async Task<IEnumerable<EditRequestResponse>> GetPendingEditRequestsAsync(int schoolId)
        {
            var reqs = await _context.StudentEditRequests
                .Include(r => r.Student)
                .Where(r => r.Student.SchoolId == schoolId && r.Status == "Pending")
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reqs.Select(r => MapToEditResponse(r, r.Student.StudentCode));
        }

        public async Task ReviewEditRequestAsync(int schoolId, int requestId, int adminUserId, ReviewEditRequest request)
        {
            var editReq = await _context.StudentEditRequests
                .Include(r => r.Student)
                    .ThenInclude(s => s.Identity)
                .Include(r => r.Student)
                    .ThenInclude(s => s.Profile)
                .Include(r => r.Student)
                    .ThenInclude(s => s.Contact)
                .Include(r => r.Student)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (editReq == null) throw new KeyNotFoundException("Không tìm thấy yêu cầu.");
            if (editReq.Student.SchoolId != schoolId) throw new UnauthorizedAccessException("Bạn không có quyền duyệt sinh viên của trường khác.");
            if (editReq.Status != "Pending") throw new InvalidOperationException("Yêu cầu này đã được xử lý trước đó.");

            editReq.Status = request.Status;
            editReq.AdminComment = request.AdminComment;
            editReq.UpdatedAt = DateTime.UtcNow;

            // Nếu Duyệt -> Cập nhật Database và Ghi Audit Log
            if (request.Status == "Approved")
            {
                if (editReq.Student.Identity == null)
                {
                    editReq.Student.Identity = new StudentIdentity { StudentId = editReq.Student.Id };
                    _context.StudentIdentities.Add(editReq.Student.Identity);
                }

                if (editReq.Student.Profile == null)
                {
                    editReq.Student.Profile = new StudentProfile { StudentId = editReq.Student.Id };
                    _context.StudentProfiles.Add(editReq.Student.Profile);
                }

                if (editReq.Student.Contact == null)
                {
                    editReq.Student.Contact = new StudentContact { StudentId = editReq.Student.Id };
                    _context.StudentContacts.Add(editReq.Student.Contact);
                }

                // Lưu giá trị cũ
                var oldValues = new
                {
                    editReq.Student.Identity.FullName,
                    editReq.Student.Identity.DateOfBirth,
                    editReq.Student.Identity.Gender
                };

                // Chỉ mở khóa nếu sinh viên có chọn mục "Xin mở khóa bảo vệ hồ sơ"
                bool shouldUnlock = !string.IsNullOrEmpty(editReq.Reason) &&
                                   (editReq.Reason.Contains("mở khóa") || editReq.Reason.Contains("UnlockProfile"));

                editReq.Student.Identity.IsLocked = shouldUnlock ? false : true;
                editReq.Student.Identity.FullName = editReq.RequestedFullName;
                if (editReq.RequestedDateOfBirth.HasValue) editReq.Student.Identity.DateOfBirth = editReq.RequestedDateOfBirth;
                if (!string.IsNullOrEmpty(editReq.RequestedGender)) editReq.Student.Identity.Gender = editReq.RequestedGender;
                editReq.Student.Identity.UpdatedAt = DateTime.UtcNow;

                // Ghi đè Profile
                editReq.Student.Profile.FullName = editReq.RequestedFullName;
                if (editReq.RequestedDateOfBirth.HasValue) editReq.Student.Profile.DateOfBirth = editReq.RequestedDateOfBirth;
                if (!string.IsNullOrEmpty(editReq.RequestedGender)) editReq.Student.Profile.Gender = editReq.RequestedGender;
                editReq.Student.Profile.UpdatedAt = DateTime.UtcNow;

                // Tự động giải mã & ghi đè tất cả các trường dữ liệu được yêu cầu từ `editReq.Reason`
                ApplyReasonUpdatesToStudent(editReq.Student, editReq.Student.Identity, editReq.Student.Profile, editReq.Student.Contact, editReq.Reason);

                // Lưu giá trị mới
                var newValues = new
                {
                    FullName = editReq.RequestedFullName,
                    DateOfBirth = editReq.RequestedDateOfBirth,
                    Gender = editReq.RequestedGender,
                    Reason = editReq.Reason
                };

                // Khởi tạo Audit Log
                _context.AuditLogs.Add(new AuditLog
                {
                    UserId = adminUserId,
                    SchoolId = schoolId,
                    Action = "Approve CCCD Edit Request",
                    EntityName = "StudentIdentity",
                    EntityId = editReq.Student.Identity.Id.ToString(),
                    OldValue = JsonSerializer.Serialize(oldValues),
                    NewValue = JsonSerializer.Serialize(newValues),
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }

        private void ApplyReasonUpdatesToStudent(Student student, StudentIdentity identity, StudentProfile profile, StudentContact contact, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) return;

            var lines = reason.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (!line.StartsWith("•")) continue;

                var parts = line.Substring(1).Split("=> Giá trị đề xuất mới:");
                if (parts.Length != 2) continue;

                var label = parts[0].Trim();
                var val = parts[1].Trim();
                if (string.IsNullOrEmpty(val) || val == "Chưa nhập giá trị mới") continue;

                if (label.Contains("Số CCCD"))
                {
                    identity.IdNumber = val;
                }
                else if (label.Contains("Quê quán"))
                {
                    identity.PlaceOfOrigin = val;
                    profile.PlaceOfBirth = val;
                }
                else if (label.Contains("thường trú"))
                {
                    identity.PlaceOfResidence = val;
                    contact.Address = val;
                }
                else if (label.Contains("Nơi cấp"))
                {
                    identity.IssuePlace = val;
                }
                else if (label.Contains("Ngày cấp"))
                {
                    if (DateTime.TryParse(val, out var parsedIssueDate))
                    {
                        identity.IssueDate = parsedIssueDate;
                    }
                }
                else if (label.Contains("Dân tộc"))
                {
                    profile.Ethnicity = val;
                }
                else if (label.Contains("Quốc tịch"))
                {
                    identity.Nationality = val;
                    profile.Nationality = val;
                }
                else if (label.Contains("Email cá nhân"))
                {
                    if (student.User != null) student.User.Email = val;
                }
                else if (label.Contains("Số điện thoại di động") || label.Contains("Số điện thoại") || label.Contains("SĐT"))
                {
                    if (!label.Contains("người thân") && !label.Contains("chủ hộ"))
                    {
                        contact.PhoneNumber = val;
                    }
                }
                else if (label.Contains("Họ tên phụ huynh") || label.Contains("người thân"))
                {
                    if (label.Contains("SĐT") || label.Contains("Số điện thoại"))
                    {
                        contact.GuardianPhoneNumber = val;
                    }
                    else
                    {
                        contact.GuardianName = val;
                    }
                }
                else if (label.Contains("Mối quan hệ"))
                {
                    contact.GuardianRelationship = val;
                }
                else if (label.Contains("tạm trú"))
                {
                    contact.TemporaryAddress = val;
                }
            }
        }

        private EditRequestResponse MapToEditResponse(StudentEditRequest req, string studentCode)
        {
            return new EditRequestResponse
            {
                Id = req.Id,
                StudentId = req.StudentId,
                StudentCode = studentCode,
                RequestedFullName = req.RequestedFullName,
                RequestedDateOfBirth = req.RequestedDateOfBirth,
                RequestedGender = req.RequestedGender,
                Reason = req.Reason,
                Status = req.Status,
                AdminComment = req.AdminComment,
                CreatedAt = req.CreatedAt
            };
        }
    }
}