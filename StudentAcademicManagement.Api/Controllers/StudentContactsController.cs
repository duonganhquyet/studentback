using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentAcademicManagement.Application.DTOs.Students;
using StudentAcademicManagement.Application.Interfaces;
using StudentAcademicManagement.Infrastructure.Persistence;
using System.Security.Claims;

namespace StudentAcademicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/student-contacts")]
    [Authorize(Roles = "SchoolAdmin,Student")]
    public class StudentContactsController : ControllerBase
    {
        private readonly IStudentContactService _contactService;
        private readonly ApplicationDbContext _context;

        public StudentContactsController(IStudentContactService contactService, ApplicationDbContext context)
        {
            _contactService = contactService;
            _context = context;
        }

        private async Task<int?> GetSchoolIdAsync()
        {
            var claim = User.FindFirst("SchoolId")?.Value;
            if (!string.IsNullOrEmpty(claim) && int.TryParse(claim, out int schoolId)) return schoolId;

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
                if (student != null) return student.SchoolId;

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user != null && user.SchoolId.HasValue) return user.SchoolId.Value;
            }
            return null;
        }

        [HttpGet("{studentId}")]
        public async Task<IActionResult> GetContact(int studentId)
        {
            try
            {
                var schoolId = await GetSchoolIdAsync();
                if (!schoolId.HasValue) return BadRequest(new { message = "Không tìm thấy thông tin Trường học." });

                if (User.IsInRole("Student"))
                {
                    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                    var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
                    if (student == null || student.Id != studentId)
                    {
                        return StatusCode(403, new { message = "Bạn chỉ có quyền xem hồ sơ của chính mình." });
                    }
                }

                return Ok(await _contactService.GetContactByStudentIdAsync(schoolId.Value, studentId));
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPut("{studentId}")]
        public async Task<IActionResult> UpdateContact(int studentId, [FromBody] UpdateStudentContactRequest request)
        {
            try
            {
                var schoolId = await GetSchoolIdAsync();
                if (!schoolId.HasValue) return BadRequest(new { message = "Không tìm thấy thông tin Trường học." });

                if (User.IsInRole("Student"))
                {
                    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                    var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
                    if (student == null || student.Id != studentId)
                    {
                        return StatusCode(403, new { message = "Bạn chỉ có quyền chỉnh sửa hồ sơ của chính mình." });
                    }
                }

                return Ok(await _contactService.UpdateContactAsync(schoolId.Value, studentId, request));
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
    }
}