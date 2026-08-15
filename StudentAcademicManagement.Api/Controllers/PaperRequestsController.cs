using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentAcademicManagement.Application.DTOs.Students;
using StudentAcademicManagement.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StudentAcademicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/paper-requests")]
    public class PaperRequestsController : ControllerBase
    {
        private readonly IPaperRequestService _paperRequestService;

        public PaperRequestsController(IPaperRequestService paperRequestService)
        {
            _paperRequestService = paperRequestService;
        }

        // ========================== DÀNH CHO SINH VIÊN ==========================
        [HttpGet("my")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyPaperRequests()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var result = await _paperRequestService.GetMyPaperRequestsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPost("my")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> CreatePaperRequest([FromBody] CreatePaperRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var result = await _paperRequestService.CreatePaperRequestAsync(userId, request);
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Lỗi hệ thống.", details = ex.Message }); }
        }

        [HttpDelete("my/{id}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> DeleteMyPaperRequest(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                await _paperRequestService.DeleteMyPaperRequestAsync(userId, id);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Lỗi hệ thống.", details = ex.Message }); }
        }


        // ========================== DÀNH CHO ADMIN ==========================
        [HttpGet]
        [Authorize(Roles = "SchoolAdmin")]
        public async Task<IActionResult> GetAllPaperRequests()
        {
            try
            {
                var schoolIdStr = User.FindFirst("SchoolId")?.Value;
                if (string.IsNullOrEmpty(schoolIdStr) || !int.TryParse(schoolIdStr, out int schoolId))
                    return BadRequest(new { message = "Tài khoản của bạn chưa được liên kết với Trường học nào (SchoolId)." });

                var result = await _paperRequestService.GetAllPaperRequestsAsync(schoolId);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPatch("{id}/review")]
        [Authorize(Roles = "SchoolAdmin")]
        public async Task<IActionResult> ReviewPaperRequest(int id, [FromBody] ReviewPaperRequest request)
        {
            try
            {
                var schoolIdStr = User.FindFirst("SchoolId")?.Value;
                if (string.IsNullOrEmpty(schoolIdStr) || !int.TryParse(schoolIdStr, out int schoolId))
                    return BadRequest(new { message = "Tài khoản của bạn chưa được liên kết với Trường học nào (SchoolId)." });

                await _paperRequestService.ReviewPaperRequestAsync(schoolId, id, request);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
    }
}
