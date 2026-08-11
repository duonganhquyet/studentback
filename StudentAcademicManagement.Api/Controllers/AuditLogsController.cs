using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentAcademicManagement.Application.DTOs.AuditLogs;
using StudentAcademicManagement.Application.Interfaces;
using System.Security.Claims;

namespace StudentAcademicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/audit-logs")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogsController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [HttpGet("system")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetSystemLogs([FromQuery] AuditLogFilterRequest request)
        {
            try
            {
                var result = await _auditLogService.GetSystemAuditLogsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống.", details = ex.Message });
            }
        }

        [HttpGet("school")]
        [Authorize(Roles = "SchoolAdmin")]
        public async Task<IActionResult> GetSchoolLogs([FromQuery] AuditLogFilterRequest request)
        {
            try
            {
                var schoolId = int.Parse(User.FindFirst("SchoolId")!.Value);
                var result = await _auditLogService.GetSchoolAuditLogsAsync(schoolId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống.", details = ex.Message });
            }
        }
    }
}