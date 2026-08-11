using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentAcademicManagement.Application.Interfaces;
using System.Security.Claims;

namespace StudentAcademicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardsController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardsController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("superadmin")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetSuperAdminDashboard()
        {
            return Ok(await _dashboardService.GetSuperAdminDashboardAsync());
        }

        [HttpGet("schooladmin")]
        [Authorize(Roles = "SchoolAdmin")]
        public async Task<IActionResult> GetSchoolAdminDashboard()
        {
            var schoolId = int.Parse(User.FindFirst("SchoolId")!.Value);
            return Ok(await _dashboardService.GetSchoolAdminDashboardAsync(schoolId));
        }

        [HttpGet("student")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetStudentDashboard()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            return Ok(await _dashboardService.GetStudentDashboardAsync(userId));
        }
    }
}