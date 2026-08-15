using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StudentAcademicManagement.Application.DTOs.Auth;
using StudentAcademicManagement.Application.Interfaces;
using StudentAcademicManagement.Domain.Entities;
using StudentAcademicManagement.Infrastructure.Persistence;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StudentAcademicManagement.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var input = (request.Email ?? string.Empty).Trim();

            // Tìm tài khoản theo Email hoặc MSSV
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == input.ToLower() ||
                    _context.Students.Any(s => s.UserId == u.Id && s.StudentCode.ToLower() == input.ToLower())
                );

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Tên đăng nhập hoặc mật khẩu không đúng.");

            string? studentCode = null;
            int? studentId = null;
            if (user.Role.Name == "Student")
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);
                if (student == null)
                {
                    throw new UnauthorizedAccessException("Tài khoản Sinh viên chưa được đăng ký trong hệ thống.");
                }
                
                if (student.AcademicStatus == "Thôi học")
                {
                    throw new UnauthorizedAccessException("Tài khoản đã bị khóa do sinh viên có trạng thái 'Thôi học'.");
                }
                studentCode = student.StudentCode;
                studentId = student.Id;
            }

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Tài khoản đã bị khóa.");

            var token = await GenerateJwtTokenAsync(user);

            return new AuthResponse
            {
                Token = token,
                Email = user.Email,
                Role = user.Role.Name,
                StudentCode = studentCode,
                StudentId = studentId,
                SchoolId = user.SchoolId,
                IsFirstLogin = user.IsFirstLogin
            };
        }

        public async Task<string> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new KeyNotFoundException("Không tìm thấy tài khoản.");

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                throw new InvalidOperationException("Mật khẩu hiện tại không chính xác.");

            // Mã hóa mật khẩu mới
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            // Tắt cờ FirstLogin nếu đây là lần đầu đổi mật khẩu
            user.IsFirstLogin = false;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Sinh Token mới có IsFirstLogin = false
            return await GenerateJwtTokenAsync(user);
        }

        private async Task<string> GenerateJwtTokenAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.Name),
                new Claim("IsFirstLogin", user.IsFirstLogin.ToString())
            };

            if (user.Role.Name == "Student")
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);
                if (student != null)
                {
                    claims.Add(new Claim("StudentCode", student.StudentCode));
                    claims.Add(new Claim("SchoolId", student.SchoolId.ToString()));
                    claims.Add(new Claim("StudentId", student.Id.ToString())); // Added StudentId claim
                }
            }

            if (user.SchoolId.HasValue && !claims.Any(c => c.Type == "SchoolId"))
            {
                claims.Add(new Claim("SchoolId", user.SchoolId.Value.ToString()));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_configuration["JwtSettings:ExpirationInMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}