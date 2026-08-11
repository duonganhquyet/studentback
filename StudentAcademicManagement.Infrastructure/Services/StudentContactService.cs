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

            return MapToResponse(student);
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

            student.Contact.PhoneNumber = request.PhoneNumber;
            student.Contact.Address = request.Address;
            student.Contact.TemporaryAddress = request.TemporaryAddress;
            student.Contact.GuardianName = request.GuardianName;
            student.Contact.GuardianPhoneNumber = request.GuardianPhoneNumber;
            student.Contact.GuardianRelationship = request.GuardianRelationship;
            student.Contact.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToResponse(student);
        }

        private StudentContactResponse MapToResponse(Student student)
        {
            return new StudentContactResponse
            {
                Id = student.Contact?.Id ?? 0,
                StudentId = student.Id,
                StudentCode = student.StudentCode,
                PhoneNumber = student.Contact?.PhoneNumber,
                Address = student.Contact?.Address,
                TemporaryAddress = student.Contact?.TemporaryAddress,
                GuardianName = student.Contact?.GuardianName,
                GuardianPhoneNumber = student.Contact?.GuardianPhoneNumber,
                GuardianRelationship = student.Contact?.GuardianRelationship
            };
        }
    }
}