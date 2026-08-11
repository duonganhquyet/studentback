using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentAcademicManagement.Application.DTOs.Students;
using StudentAcademicManagement.Application.Interfaces;

namespace StudentAcademicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/student-contacts")]
    [Authorize(Roles = "SchoolAdmin")]
    public class StudentContactsController : ControllerBase
    {
        private readonly IStudentContactService _contactService;

        public StudentContactsController(IStudentContactService contactService)
        {
            _contactService = contactService;
        }

        [HttpGet("{studentId}")]
        public async Task<IActionResult> GetContact(int studentId)
        {
            try
            {
                var schoolId = int.Parse(User.FindFirst("SchoolId")!.Value);
                return Ok(await _contactService.GetContactByStudentIdAsync(schoolId, studentId));
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpPut("{studentId}")]
        public async Task<IActionResult> UpdateContact(int studentId, [FromBody] UpdateStudentContactRequest request)
        {
            try
            {
                var schoolId = int.Parse(User.FindFirst("SchoolId")!.Value);
                return Ok(await _contactService.UpdateContactAsync(schoolId, studentId, request));
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }
    }
}