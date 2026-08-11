using Microsoft.Extensions.Logging;
using StudentAcademicManagement.Application.Interfaces;

namespace StudentAcademicManagement.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string toEmail, string subject, string body)
        {
            // Trong môi trường thực tế, cấu hình SMTP (MailKit hoặc SendGrid) ở đây.
            // Để phục vụ đồ án chạy local mượt mà, ta log ra console để kiểm tra.
            _logger.LogInformation("========== EMAIL SENT ==========");
            _logger.LogInformation($"To: {toEmail}");
            _logger.LogInformation($"Subject: {subject}");
            _logger.LogInformation($"Body: {body}");
            _logger.LogInformation("================================");

            return Task.CompletedTask;
        }
    }
}