using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentAcademicManagement.Application.DTOs.SchoolAdmins;
using StudentAcademicManagement.Application.Interfaces;

namespace StudentAcademicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/school-admins")]
    [Authorize(Roles = "SuperAdmin")] // Phân quyền: BẮT BUỘC SuperAdmin
    public class SchoolAdminsController : ControllerBase
    {
        private readonly ISchoolAdminService _schoolAdminService;

        public SchoolAdminsController(ISchoolAdminService schoolAdminService)
        {
            _schoolAdminService = schoolAdminService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSchoolAdmins()
        {
            var list = await _schoolAdminService.GetAllSchoolAdminsAsync();
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSchoolAdmin([FromBody] CreateSchoolAdminRequest request)
        {
            try
            {
                var response = await _schoolAdminService.CreateSchoolAdminAsync(request);
                return StatusCode(201, response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống.", details = ex.Message });
            }
        }
    }
}