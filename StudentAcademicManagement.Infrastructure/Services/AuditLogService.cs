using Microsoft.EntityFrameworkCore;
using StudentAcademicManagement.Application.DTOs.AuditLogs;
using StudentAcademicManagement.Application.DTOs.Common;
using StudentAcademicManagement.Application.Interfaces;
using StudentAcademicManagement.Domain.Entities;
using StudentAcademicManagement.Infrastructure.Persistence;

namespace StudentAcademicManagement.Infrastructure.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationDbContext _context;

        public AuditLogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(int? userId, int? schoolId, string action, string entityName, string? entityId = null, string? oldValue = null, string? newValue = null)
        {
            var log = new AuditLog
            {
                UserId = userId,
                SchoolId = schoolId,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                OldValue = oldValue,
                NewValue = newValue,
                CreatedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<PagedResult<AuditLogResponse>> GetSystemAuditLogsAsync(AuditLogFilterRequest request)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(a => a.Action.ToLower().Contains(term) || a.EntityName.ToLower().Contains(term) || (a.NewValue != null && a.NewValue.ToLower().Contains(term)));
            }

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(a => new
                {
                    Log = a,
                    UserEmail = _context.Users.Where(u => u.Id == a.UserId).Select(u => u.Email).FirstOrDefault(),
                    SchoolName = _context.Schools.Where(s => s.Id == a.SchoolId).Select(s => s.SchoolName).FirstOrDefault()
                })
                .ToListAsync();

            var items = logs.Select(x => new AuditLogResponse
            {
                Id = x.Log.Id,
                UserId = x.Log.UserId,
                UserEmail = x.UserEmail ?? "SuperAdmin",
                SchoolId = x.Log.SchoolId,
                SchoolName = x.SchoolName ?? "Hệ thống",
                Action = x.Log.Action,
                EntityName = x.Log.EntityName,
                EntityId = x.Log.EntityId,
                OldValue = x.Log.OldValue,
                NewValue = x.Log.NewValue,
                IpAddress = x.Log.IpAddress,
                CreatedAt = x.Log.CreatedAt
            }).ToList();

            return new PagedResult<AuditLogResponse>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<PagedResult<AuditLogResponse>> GetSchoolAuditLogsAsync(int schoolId, AuditLogFilterRequest request)
        {
            var query = _context.AuditLogs.Where(a => a.SchoolId == schoolId).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(a => a.Action.ToLower().Contains(term) || a.EntityName.ToLower().Contains(term) || (a.NewValue != null && a.NewValue.ToLower().Contains(term)));
            }

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(a => new
                {
                    Log = a,
                    UserEmail = _context.Users.Where(u => u.Id == a.UserId).Select(u => u.Email).FirstOrDefault(),
                    SchoolName = _context.Schools.Where(s => s.Id == a.SchoolId).Select(s => s.SchoolName).FirstOrDefault()
                })
                .ToListAsync();

            var items = logs.Select(x => new AuditLogResponse
            {
                Id = x.Log.Id,
                UserId = x.Log.UserId,
                UserEmail = x.UserEmail ?? "System",
                SchoolId = x.Log.SchoolId,
                SchoolName = x.SchoolName ?? "System Level",
                Action = x.Log.Action,
                EntityName = x.Log.EntityName,
                EntityId = x.Log.EntityId,
                OldValue = x.Log.OldValue,
                NewValue = x.Log.NewValue,
                IpAddress = x.Log.IpAddress,
                CreatedAt = x.Log.CreatedAt
            }).ToList();

            return new PagedResult<AuditLogResponse>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}