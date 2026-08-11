using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentAcademicManagement.Application.DTOs.SpecialCategories;
using StudentAcademicManagement.Application.Interfaces;
using StudentAcademicManagement.Infrastructure.Persistence;
using System.Security.Claims;

namespace StudentAcademicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/special-categories")]
    public class SpecialCategoriesController : ControllerBase
    {
        private readonly ISpecialCategoryService _categoryService;
        private readonly ApplicationDbContext _context;

        public SpecialCategoriesController(ISpecialCategoryService categoryService, ApplicationDbContext context)
        {
            _categoryService = categoryService;
            _context = context;
        }

        private async Task<int?> GetSchoolIdAsync()
        {
            var claim = User.FindFirst("SchoolId")?.Value;
            if (!string.IsNullOrEmpty(claim) && int.TryParse(claim, out int schoolId))
            {
                return schoolId;
            }

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

        // --- SCHOOL ADMIN & STUDENT ENDPOINTS ---
        [HttpGet]
        [Authorize(Roles = "SchoolAdmin,Student")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var schoolId = await GetSchoolIdAsync();
                if (!schoolId.HasValue) return BadRequest(new { message = "Không tìm thấy thông tin Trường học của tài khoản." });

                return Ok(await _categoryService.GetCategoriesAsync(schoolId.Value));
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPost]
        [Authorize(Roles = "SchoolAdmin")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateSpecialCategoryRequest request)
        {
            try
            {
                var schoolId = await GetSchoolIdAsync();
                if (!schoolId.HasValue) return BadRequest(new { message = "Không tìm thấy thông tin Trường học của tài khoản." });

                return Ok(await _categoryService.CreateCategoryAsync(schoolId.Value, request));
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPatch("{id}/toggle-status")]
        [Authorize(Roles = "SchoolAdmin")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var schoolId = await GetSchoolIdAsync();
                if (!schoolId.HasValue) return BadRequest(new { message = "Không tìm thấy thông tin Trường học của tài khoản." });

                await _categoryService.ToggleCategoryStatusAsync(schoolId.Value, id);
                return NoContent();
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpGet("registrations/pending")]
        [Authorize(Roles = "SchoolAdmin")]
        public async Task<IActionResult> GetPendingRegistrations()
        {
            try
            {
                var schoolId = await GetSchoolIdAsync();
                if (!schoolId.HasValue) return BadRequest(new { message = "Không tìm thấy thông tin Trường học của tài khoản." });

                return Ok(await _categoryService.GetPendingRegistrationsAsync(schoolId.Value));
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPatch("registrations/{id}/review")]
        [Authorize(Roles = "SchoolAdmin")]
        public async Task<IActionResult> ReviewRegistration(int id, [FromBody] ReviewSpecialCategoryRequest request)
        {
            try
            {
                var schoolId = await GetSchoolIdAsync();
                if (!schoolId.HasValue) return BadRequest(new { message = "Không tìm thấy thông tin Trường học của tài khoản." });

                await _categoryService.ReviewRegistrationAsync(schoolId.Value, id, request);
                return NoContent();
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        // --- STUDENT ENDPOINTS ---
        [HttpPost("my-registrations")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Register([FromForm] RegisterSpecialCategoryRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var result = await _categoryService.RegisterSpecialCategoryAsync(userId, request);
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpGet("my-registrations")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyRegistrations()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                return Ok(await _categoryService.GetMyRegistrationsAsync(userId));
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
    }
}