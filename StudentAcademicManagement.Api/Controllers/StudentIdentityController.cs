using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentAcademicManagement.Application.DTOs.Students;
using StudentAcademicManagement.Application.Interfaces;
using System.Security.Claims;

namespace StudentAcademicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/student-identities")]
    public class StudentIdentityController : ControllerBase
    {
        private readonly IStudentIdentityService _identityService;

        public StudentIdentityController(IStudentIdentityService identityService)
        {
            _identityService = identityService;
        }

        // =========================================================================
        // ENDPOINTS DÀNH CHO SINH VIÊN (STUDENT)
        // =========================================================================

        [HttpGet("my")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyIdentity()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var result = await _identityService.GetIdentityAsync(userId);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPost("process-ocr")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> ProcessOcr([FromForm] UploadCccdRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var response = await _identityService.UploadAndProcessCccdAsync(userId, request);
                return Ok(response);
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Lỗi hệ thống.", details = ex.Message }); }
        }

        [HttpPost("confirm")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> ConfirmIdentity([FromBody] ConfirmCccdRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                await _identityService.ConfirmAndLockIdentityAsync(userId, request);
                return Ok(new { message = "Xác thực thành công. Dữ liệu của bạn đã được khóa bảo vệ." });
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Lỗi hệ thống.", details = ex.Message }); }
        }

        [HttpPost("my/edit-requests")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> CreateEditRequest([FromBody] CreateEditRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var response = await _identityService.CreateEditRequestAsync(userId, request);
                return Ok(response);
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Lỗi hệ thống.", details = ex.Message }); }
        }

        [HttpGet("my/edit-requests/pending")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyPendingEditRequest()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var result = await _identityService.GetMyPendingEditRequestAsync(userId);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }


        // =========================================================================
        // ENDPOINTS DÀNH CHO QUẢN TRỊ TRƯỜNG HỌC (SCHOOL ADMIN)
        // =========================================================================

        [HttpGet("edit-requests")]
        [Authorize(Roles = "SchoolAdmin")]
        public async Task<IActionResult> GetPendingEditRequests()
        {
            try
            {
                var schoolIdClaim = User.FindFirst("SchoolId")?.Value;
                if (string.IsNullOrEmpty(schoolIdClaim) || !int.TryParse(schoolIdClaim, out int schoolId))
                {
                    return BadRequest(new { message = "Tài khoản của bạn chưa được liên kết với Trường học nào (SchoolId)." });
                }
                var reqs = await _identityService.GetPendingEditRequestsAsync(schoolId);
                return Ok(reqs);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPatch("edit-requests/{id}/review")]
        [Authorize(Roles = "SchoolAdmin")]
        public async Task<IActionResult> ReviewEditRequest(int id, [FromBody] ReviewEditRequest request)
        {
            try
            {
                var schoolIdClaim = User.FindFirst("SchoolId")?.Value;
                if (string.IsNullOrEmpty(schoolIdClaim) || !int.TryParse(schoolIdClaim, out int schoolId))
                {
                    return BadRequest(new { message = "Tài khoản của bạn chưa được liên kết với Trường học nào (SchoolId)." });
                }
                var adminUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                await _identityService.ReviewEditRequestAsync(schoolId, id, adminUserId, request);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Lỗi hệ thống.", details = ex.Message }); }
        }
    }
}