using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentAcademicManagement.Application.DTOs.Schools;
using StudentAcademicManagement.Application.Interfaces;

namespace StudentAcademicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // BỎ [Authorize(Roles = "SuperAdmin")] ở đây để cấu hình quyền riêng cho từng API
    public class SchoolsController : ControllerBase
    {
        private readonly ISchoolService _schoolService;

        public SchoolsController(ISchoolService schoolService)
        {
            _schoolService = schoolService;
        }

        // ================= ENDPOINT DÀNH CHO MULTI-SCHOOL (SchoolAdmin & Student) =================
        [HttpGet("current")]
        [Authorize(Roles = "SchoolAdmin,Student")]
        public async Task<IActionResult> GetCurrentSchool()
        {
            // Lấy SchoolId từ JWT Token
            var schoolIdClaim = User.FindFirst("SchoolId")?.Value;
            if (string.IsNullOrEmpty(schoolIdClaim)) return StatusCode(403, new { message = "Forbidden" });

            var schoolId = int.Parse(schoolIdClaim);
            try
            {
                var school = await _schoolService.GetSchoolByIdAsync(schoolId);
                return Ok(school);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ================= CÁC ENDPOINTS CỦA SUPER ADMIN =================
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateSchool([FromForm] CreateSchoolRequest request)
        {
            try { return StatusCode(201, await _schoolService.CreateSchoolAsync(request)); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetAllSchools()
        {
            var schools = await _schoolService.GetAllSchoolsAsync();
            return Ok(schools);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetSchoolById(int id)
        {
            try { return Ok(await _schoolService.GetSchoolByIdAsync(id)); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateSchool(int id, [FromForm] UpdateSchoolRequest request)
        {
            try { return Ok(await _schoolService.UpdateSchoolAsync(id, request)); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpPatch("{id}/activate")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> ActivateSchool(int id)
        {
            try
            {
                await _schoolService.ChangeSchoolStatusAsync(id, true);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpPatch("{id}/deactivate")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeactivateSchool(int id)
        {
            try
            {
                await _schoolService.ChangeSchoolStatusAsync(id, false);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }
    }
}