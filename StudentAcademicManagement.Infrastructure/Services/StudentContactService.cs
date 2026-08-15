using Microsoft.EntityFrameworkCore;
using StudentAcademicManagement.Application.DTOs.Students;
using StudentAcademicManagement.Application.Interfaces;
using StudentAcademicManagement.Domain.Entities;
using StudentAcademicManagement.Infrastructure.Persistence;

namespace StudentAcademicManagement.Infrastructure.Services
{
    public class StudentContactService : IStudentContactService
    {
        private readonly ApplicationDbContext _context;

        public StudentContactService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<StudentContactResponse> GetContactByStudentIdAsync(int schoolId, int studentId)
        {
            var student = await _context.Students
                .Include(s => s.Contact)
                .FirstOrDefaultAsync(s => s.Id == studentId && s.SchoolId == schoolId);

            if (student == null) throw new KeyNotFoundException("Không tìm thấy sinh viên trong trường này.");

            var familyMembers = await _context.FamilyMembers
                .Where(f => f.StudentId == studentId)
                .ToListAsync();

            return MapToResponse(student, familyMembers);
        }

        public async Task<StudentContactResponse> UpdateContactAsync(int schoolId, int studentId, UpdateStudentContactRequest request)
        {
            var student = await _context.Students
                .Include(s => s.Contact)
                .FirstOrDefaultAsync(s => s.Id == studentId && s.SchoolId == schoolId);

            if (student == null) throw new KeyNotFoundException("Không tìm thấy sinh viên.");

            // Lazy Init: Nếu chưa có record Contact thì tạo mới
            if (student.Contact == null)
            {
                student.Contact = new StudentContact { StudentId = student.Id };
                _context.StudentContacts.Add(student.Contact);
            }

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                if (await _context.StudentContacts.AnyAsync(c => c.PhoneNumber == request.PhoneNumber && c.StudentId != student.Id))
                    throw new InvalidOperationException($"Số điện thoại {request.PhoneNumber} đã được sử dụng bởi một sinh viên khác.");
            }

            student.Contact.PhoneNumber = request.PhoneNumber;
            student.Contact.Address = request.Address;
            student.Contact.TemporaryAddress = request.TemporaryAddress;
            student.Contact.ResidenceType = request.ResidenceType;
            student.Contact.LandlordName = request.LandlordName;
            student.Contact.LandlordPhone = request.LandlordPhone;
            student.Contact.GuardianName = request.GuardianName;
            student.Contact.GuardianPhoneNumber = request.GuardianPhoneNumber;
            student.Contact.GuardianRelationship = request.GuardianRelationship;
            student.Contact.UpdatedAt = DateTime.UtcNow;

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

                // Xóa những người cũ không còn trong danh sách gửi lên (dựa trên tên hoặc sđt)
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

            await _context.SaveChangesAsync();

            var familyMembers = await _context.FamilyMembers
                .Where(f => f.StudentId == studentId)
                .ToListAsync();

            return MapToResponse(student, familyMembers);
        }

        private StudentContactResponse MapToResponse(Student student, List<StudentFamilyMember> familyMembers)
        {
            return new StudentContactResponse
            {
                Id = student.Contact?.Id ?? 0,
                StudentId = student.Id,
                StudentCode = student.StudentCode,
                PhoneNumber = student.Contact?.PhoneNumber,
                Address = student.Contact?.Address,
                TemporaryAddress = student.Contact?.TemporaryAddress,
                ResidenceType = student.Contact?.ResidenceType,
                LandlordName = student.Contact?.LandlordName,
                LandlordPhone = student.Contact?.LandlordPhone,
                GuardianName = student.Contact?.GuardianName,
                GuardianPhoneNumber = student.Contact?.GuardianPhoneNumber,
                GuardianRelationship = student.Contact?.GuardianRelationship,
                FamilyMembers = familyMembers.Select(f => new FamilyMemberDto
                {
                    Id = f.Id,
                    FullName = f.FullName,
                    Relationship = f.Relationship,
                    Nationality = f.Nationality,
                    BirthYear = f.BirthYear,
                    Job = f.Job,
                    Position = f.Position,
                    Company = f.Company,
                    Ethnicity = f.Ethnicity,
                    Religion = f.Religion,
                    PhoneNumber = f.PhoneNumber,
                    PermanentAddress = f.PermanentAddress,
                    CurrentAddress = f.CurrentAddress,
                    IsEmergencyContact = f.IsEmergencyContact,
                    IsAlumni = f.IsAlumni
                }).ToList()
            };
        }
    }
}