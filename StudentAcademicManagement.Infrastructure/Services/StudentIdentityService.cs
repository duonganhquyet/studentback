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
                BackImageUrl = student.Identity?.BackImageUrl,
                Nationality = student.Identity?.Nationality ?? student.Profile?.Nationality
            };
        }

        public async Task<StudentIdentityResponse?> GetIdentityByStudentIdAsync(int schoolId, int studentId)
        {
            var student = await _context.Students
                .Include(s => s.Profile)
                .Include(s => s.Identity)
                .FirstOrDefaultAsync(s => s.Id == studentId && s.SchoolId == schoolId);

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
                BackImageUrl = student.Identity?.BackImageUrl,
                Nationality = student.Identity?.Nationality ?? student.Profile?.Nationality
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

            if (!string.IsNullOrWhiteSpace(request.IdNumber))
            {
                if (await _context.StudentIdentities.AnyAsync(i => i.IdNumber == request.IdNumber && i.StudentId != student.Id))
                    throw new InvalidOperationException($"Số căn cước công dân (CCCD) {request.IdNumber} đã được sử dụng bởi một sinh viên khác.");
            }

            // 1. Cập nhật Identity chuẩn
            student.Identity.IdNumber = request.IdNumber;
            student.Identity.FullName = request.FullName;
            student.Identity.DateOfBirth = request.Dob;
            student.Identity.Gender = request.Gender;
            student.Identity.PlaceOfOrigin = request.PlaceOfOrigin;
            student.Identity.PlaceOfResidence = request.Address;
            student.Identity.IssueDate = request.IssueDate;
            student.Identity.IssuePlace = request.IssuePlace;
            student.Identity.VerificationStatus = "Verified";

            // 1.5 Lưu danh sách gia đình
            if (request.FamilyMembers != null && request.FamilyMembers.Any())
            {
                var existingMembers = await _context.FamilyMembers.Where(f => f.StudentId == student.Id).ToListAsync();

                // Lọc trùng tên hoặc số điện thoại trong request (giữ bản ghi cuối/mới nhất)
                var uniqueNewMembers = new List<FamilyMemberDto>();
                foreach (var m in request.FamilyMembers.Where(x => !string.IsNullOrWhiteSpace(x.FullName)).Reverse())
                {
                    if (!uniqueNewMembers.Any(u =>
                        u.FullName.Trim().Equals(m.FullName.Trim(), StringComparison.OrdinalIgnoreCase) ||
                        (!string.IsNullOrWhiteSpace(u.PhoneNumber) && !string.IsNullOrWhiteSpace(m.PhoneNumber) && u.PhoneNumber == m.PhoneNumber)))
                    {
                        uniqueNewMembers.Add(m);
                    }
                }
                uniqueNewMembers.Reverse();

                // Xóa những người cũ không còn trong danh sách gửi lên
                var namesToKeep = uniqueNewMembers.Select(m => m.FullName.Trim().ToLower()).ToList();
                var phonesToKeep = uniqueNewMembers.Where(m => !string.IsNullOrWhiteSpace(m.PhoneNumber)).Select(m => m.PhoneNumber).ToList();
                var membersToRemove = existingMembers.Where(e =>
                    !namesToKeep.Contains(e.FullName.Trim().ToLower()) &&
                    (string.IsNullOrWhiteSpace(e.PhoneNumber) || !phonesToKeep.Contains(e.PhoneNumber))
                ).ToList();
                _context.FamilyMembers.RemoveRange(membersToRemove);

                foreach (var m in uniqueNewMembers)
                {
                    var existings = existingMembers.Where(e =>
                        e.FullName.Trim().Equals(m.FullName.Trim(), StringComparison.OrdinalIgnoreCase) ||
                        (!string.IsNullOrWhiteSpace(e.PhoneNumber) && !string.IsNullOrWhiteSpace(m.PhoneNumber) && e.PhoneNumber == m.PhoneNumber)
                    ).ToList();
                    
                    if (existings.Any())
                    {
                        var existing = existings.First();
                        // Cập nhật người cũ
                        existing.Relationship = m.Relationship ?? string.Empty;
                        existing.Nationality = m.Nationality;
                        existing.BirthYear = m.BirthYear;
                        existing.Job = m.Job;
                        existing.Position = m.Position;
                        existing.Company = m.Company;
                        existing.Ethnicity = m.Ethnicity;
                        existing.Religion = m.Religion;
                        existing.PhoneNumber = m.PhoneNumber;
                        existing.PermanentAddress = m.PermanentAddress;
                        existing.CurrentAddress = m.CurrentAddress;
                        existing.IsEmergencyContact = m.IsEmergencyContact;
                        existing.IsAlumni = m.IsAlumni;

                        // Xóa các bản duplicate bị dư thừa trong DB cũ
                        if (existings.Count > 1)
                        {
                            _context.FamilyMembers.RemoveRange(existings.Skip(1));
                        }
                    }
                    else
                    {
                        // Thêm người mới
                        _context.FamilyMembers.Add(new StudentFamilyMember
                        {
                            StudentId = student.Id,
                            FullName = m.FullName.Trim(),
                            Relationship = m.Relationship ?? string.Empty,
                            Nationality = m.Nationality,
                            BirthYear = m.BirthYear,
                            Job = m.Job,
                            Position = m.Position,
                            Company = m.Company,
                            Ethnicity = m.Ethnicity,
                            Religion = m.Religion,
                            PhoneNumber = m.PhoneNumber,
                            PermanentAddress = m.PermanentAddress,
                            CurrentAddress = m.CurrentAddress,
                            IsEmergencyContact = m.IsEmergencyContact,
                            IsAlumni = m.IsAlumni
                        });
                    }
                }
            }

            // Lệnh Khóa bảo vệ
            student.Identity.IsLocked = true;

            // Đồng bộ dữ liệu sang Profile của sinh viên
            if (student.Profile == null)
            {
                student.Profile = new StudentProfile { StudentId = student.Id };
                _context.StudentProfiles.Add(student.Profile);
            }
            student.Profile.FullName = request.FullName;
            student.Profile.DateOfBirth = request.Dob;
            student.Profile.Gender = request.Gender;
            student.Profile.PlaceOfBirth = request.PlaceOfOrigin;

            await LogAuditAsync(userId, "STUDENT_CONFIRM_IDENTITY", $"Sinh viên xác nhận & Khóa hồ sơ (Tự động cập nhật Profile theo OCR): {request.IdNumber}");

            await _context.SaveChangesAsync();
        }

        private async Task LogAuditAsync(int userId, string action, string details)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                Action = action,
                NewValue = details,
                IpAddress = "127.0.0.1",
                CreatedAt = DateTime.UtcNow
            });
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

                // Kiểm tra tính duy nhất sau khi áp dụng Reason updates
                if (!string.IsNullOrWhiteSpace(editReq.Student.Identity.IdNumber))
                {
                    if (await _context.StudentIdentities.AnyAsync(i => i.IdNumber == editReq.Student.Identity.IdNumber && i.StudentId != editReq.Student.Id))
                        throw new InvalidOperationException($"Số căn cước công dân (CCCD) {editReq.Student.Identity.IdNumber} đã được sử dụng bởi một sinh viên khác.");
                }

                if (editReq.Student.Contact != null && !string.IsNullOrWhiteSpace(editReq.Student.Contact.PhoneNumber))
                {
                    if (await _context.StudentContacts.AnyAsync(c => c.PhoneNumber == editReq.Student.Contact.PhoneNumber && c.StudentId != editReq.Student.Id))
                        throw new InvalidOperationException($"Số điện thoại {editReq.Student.Contact.PhoneNumber} đã được sử dụng bởi một sinh viên khác.");
                }

                if (editReq.Student.User != null && !string.IsNullOrWhiteSpace(editReq.Student.User.Email))
                {
                    if (await _context.Users.AnyAsync(u => u.Email == editReq.Student.User.Email && u.Id != editReq.Student.UserId))
                        throw new InvalidOperationException($"Email {editReq.Student.User.Email} đã được sử dụng bởi một người khác.");
                }

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