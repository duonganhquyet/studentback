using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentAcademicManagement.Application.DTOs.Students;
using StudentAcademicManagement.Application.Interfaces;
using System.Security.Claims;

namespace StudentAcademicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SchoolAdmin")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        private int? GetSchoolId()
        {
            var claim = User.FindFirst("SchoolId")?.Value;
            if (!string.IsNullOrEmpty(claim) && int.TryParse(claim, out int schoolId))
            {
                return schoolId;
            }
            return null;
        }

        [HttpPost]
        public async Task<IActionResult> CreateStudent([FromBody] CreateStudentRequest request)
        {
            try
            {
                var schoolId = GetSchoolId();
                if (!schoolId.HasValue) return BadRequest(new { message = "Tài khoản chưa liên kết với Trường học nào (SchoolId). Vui lòng đăng xuất và đăng nhập lại." });

                var response = await _studentService.CreateStudentAsync(schoolId.Value, request);
                return StatusCode(201, response);
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Lỗi hệ thống.", details = ex.Message }); }
        }

        [HttpPost("import-batch")]
        public async Task<IActionResult> ImportBatchStudents([FromBody] List<BatchImportStudentItem> items)
        {
            try
            {
                var schoolId = GetSchoolId();
                if (!schoolId.HasValue) return BadRequest(new { message = "Tài khoản chưa liên kết với Trường học nào (SchoolId). Vui lòng đăng xuất và đăng nhập lại." });

                var result = await _studentService.BatchImportStudentsAsync(schoolId.Value, items);
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = "Lỗi hệ thống.", details = ex.Message }); }
        }

        [HttpGet]
        public async Task<IActionResult> GetStudents([FromQuery] StudentFilterRequest request)
        {
            try
            {
                var schoolId = GetSchoolId();
                if (!schoolId.HasValue) return BadRequest(new { message = "Tài khoản chưa liên kết với Trường học nào (SchoolId). Vui lòng đăng xuất và đăng nhập lại." });

                var result = await _studentService.GetStudentsAsync(schoolId.Value, request);
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = "Lỗi hệ thống.", details = ex.Message }); }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeStudentStatusRequest request)
        {
            try
            {
                var schoolId = GetSchoolId();
                if (!schoolId.HasValue) return BadRequest(new { message = "Tài khoản chưa liên kết với Trường học nào (SchoolId). Vui lòng đăng xuất và đăng nhập lại." });

                var adminUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                await _studentService.ChangeStudentStatusAsync(schoolId.Value, id, adminUserId, request);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Lỗi hệ thống.", details = ex.Message }); }
        }

        [HttpPut("{id}/academic-info")]
        public async Task<IActionResult> UpdateAcademicInfo(int id, [FromBody] UpdateStudentAcademicRequest request)
        {
            try
            {
                var schoolId = GetSchoolId();
                if (!schoolId.HasValue) return BadRequest(new { message = "Tài khoản chưa liên kết với Trường học nào (SchoolId). Vui lòng đăng xuất và đăng nhập lại." });

                await _studentService.UpdateStudentAcademicInfoAsync(schoolId.Value, id, request);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Lỗi hệ thống.", details = ex.Message }); }
        }
    }
}