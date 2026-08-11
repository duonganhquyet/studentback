using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentAcademicManagement.Application.DTOs.Students;
using StudentAcademicManagement.Application.Interfaces;
using System.Security.Claims;

namespace StudentAcademicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/student-documents")]
    public class StudentDocumentsController : ControllerBase
    {
        private readonly IStudentDocumentService _documentService;

        public StudentDocumentsController(IStudentDocumentService documentService)
        {
            _documentService = documentService;
        }

        // ================= ENDPOINTS DÀNH CHO STUDENT =================
        [HttpPost("my")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> UploadMyDocument([FromForm] UploadDocumentRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var response = await _documentService.UploadDocumentAsync(userId, request);
                return StatusCode(201, response);
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }

        [HttpGet("my")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyDocuments()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var docs = await _documentService.GetMyDocumentsAsync(userId);
                return Ok(docs);
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }

        [HttpDelete("my/{id}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> DeleteMyDocument(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                await _documentService.DeleteMyDocumentAsync(userId, id);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ================= ENDPOINTS DÀNH CHO SCHOOL ADMIN =================
        [HttpGet("student/{studentId}")]
        [Authorize(Roles = "SchoolAdmin")]
        public async Task<IActionResult> GetStudentDocuments(int studentId)
        {
            try
            {
                var schoolId = int.Parse(User.FindFirst("SchoolId")!.Value);
                var docs = await _documentService.GetDocumentsByStudentIdAsync(schoolId, studentId);
                return Ok(docs);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpPatch("{id}/review")]
        [Authorize(Roles = "SchoolAdmin")]
        public async Task<IActionResult> ReviewDocument(int id, [FromBody] ReviewDocumentRequest request)
        {
            try
            {
                var schoolId = int.Parse(User.FindFirst("SchoolId")!.Value);
                await _documentService.ReviewDocumentAsync(schoolId, id, request);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SchoolAdmin")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            try
            {
                var schoolId = int.Parse(User.FindFirst("SchoolId")!.Value);
                await _documentService.DeleteDocumentAsync(schoolId, id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}