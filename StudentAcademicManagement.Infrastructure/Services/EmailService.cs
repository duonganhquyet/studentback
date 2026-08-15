using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StudentAcademicManagement.Application.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace StudentAcademicManagement.Infrastructure.Services
{
    // ==============================================================================
    // 1. LỚP ĐẠI DIỆN CHO MỘT BỨC THƯ (EMAIL JOB)
    // ==============================================================================
    /// <summary>
    /// Chứa thông tin của một bức thư để đưa vào hàng đợi.
    /// Dùng DTO này giúp hệ thống lưu trữ được thư trong bộ nhớ khi chưa kịp gửi.
    /// </summary>
    public class EmailJob
    {
        public string ToEmail { get; set; } = string.Empty;   // Địa chỉ người nhận (Sinh viên)
        public string Subject { get; set; } = string.Empty;   // Tiêu đề bức thư
        public string Body { get; set; } = string.Empty;      // Nội dung bức thư
    }

    // ==============================================================================
    // 2. KHAI BÁO GIAO DIỆN HÀNG ĐỢI (QUEUE) VÀ TRIỂN KHAI BẰNG CONCURRENT QUEUE
    // ==============================================================================
    public interface IEmailQueue
    {
        void Enqueue(EmailJob job);              // Thêm thư vào cuối hàng đợi (Vào sau - Ra sau)
        bool TryDequeue(out EmailJob? job);      // Lấy thư ra từ đầu hàng đợi (Vào trước - Ra trước)
        int Count { get; }                       // Lấy số lượng thư đang chờ xử lý
    }

    /// <summary>
    /// Sử dụng ConcurrentQueue vì đây là hàng đợi an toàn trong môi trường đa luồng (Thread-safe).
    /// Đảm bảo không bị mất dữ liệu khi có nhiều Request gửi email cùng lúc.
    /// </summary>
    public class EmailQueue : IEmailQueue
    {
        private readonly ConcurrentQueue<EmailJob> _queue = new();

        public void Enqueue(EmailJob job) => _queue.Enqueue(job);
        public bool TryDequeue(out EmailJob? job) => _queue.TryDequeue(out job);
        public int Count => _queue.Count;
    }

    // ==============================================================================
    // 3. SERVICE TIẾP NHẬN YÊU CẦU GỬI (Dành cho Controller và các Service khác gọi tới)
    // ==============================================================================
    /// <summary>
    /// Thay vì gửi email thực sự (gây nghẽn hệ thống do phải chờ SMTP Server),
    /// Service này chỉ có duy nhất 1 việc: Đóng gói thư và ném vào Hàng Đợi (Queue), sau đó trả về ngay lập tức.
    /// Giúp người dùng không phải chờ đợi màn hình loading.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IEmailQueue _emailQueue;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IEmailQueue emailQueue, ILogger<EmailService> logger)
        {
            _emailQueue = emailQueue;
            _logger = logger;
        }

        public Task SendEmailAsync(string toEmail, string subject, string body)
        {
            _emailQueue.Enqueue(new EmailJob { ToEmail = toEmail, Subject = subject, Body = body });
            _logger.LogInformation($"[Email Queue] Đã thêm thư gửi tới {toEmail} vào hàng đợi FIFO. Tổng số thư đang chờ: {_emailQueue.Count}");
            return Task.CompletedTask;
        }
    }

    // ==============================================================================
    // 4. TIẾN TRÌNH CHẠY NGẦM XỬ LÝ HÀNG ĐỢI VÀ GỬI THỰC TẾ (BACKGROUND SENDER)
    // ==============================================================================
    /// <summary>
    /// Đây là một Hosted Service (tiến trình chạy ngầm vô thời hạn) kế thừa từ BackgroundService của .NET Core.
    /// Nhiệm vụ: Liên tục quét hàng đợi, nếu có thư thì lôi ra gửi, đồng thời quản lý luân phiên các tài khoản Gmail.
    /// </summary>
    public class EmailBackgroundSender : BackgroundService
    {
        private readonly IEmailQueue _emailQueue;
        private readonly IConfiguration _config;
        private readonly ILogger<EmailBackgroundSender> _logger;

        // Các biến lưu trạng thái để xoay vòng tài khoản (Load Balancing)
        private int _currentAccountIndex = 0;        // Vị trí của tài khoản đang dùng trong mảng appsettings.json
        private int _currentAccountSendCount = 0;    // Số lượng thư tài khoản này đã gửi thành công

        public EmailBackgroundSender(IEmailQueue emailQueue, IConfiguration config, ILogger<EmailBackgroundSender> logger)
        {
            _emailQueue = emailQueue;
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Hàm này tự động được kích hoạt khi Server (API) vừa khởi động và chạy vòng lặp vô tận.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[Email Background Sender] Tiến trình gửi email ngầm đã khởi động.");

            while (!stoppingToken.IsCancellationRequested)
            {
                // Thử bốc 1 lá thư từ trong Queue ra (FIFO)
                if (_emailQueue.TryDequeue(out var job) && job != null)
                {
                    bool sent = await ProcessEmailJobAsync(job);
                    
                    // Nếu gửi thất bại (lỗi mạng, sai pass...), KHÔNG ĐƯỢC BỎ MẤT THƯ
                    if (!sent)
                    {
                        // Nhét ngược lại lá thư đó vào Queue để lần sau thử lại
                        _emailQueue.Enqueue(job);
                        
                        // Nghỉ ngơi 5 giây để tránh việc bị khóa IP do thử lại quá nhanh liên tục
                        await Task.Delay(5000, stoppingToken); 
                    }
                }
                else
                {
                    // Nếu Queue rỗng (không có ai nhờ gửi thư), cho tiến trình ngủ 1 giây để tiết kiệm CPU cho máy chủ
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }

        /// <summary>
        /// Hàm lõi thực hiện kết nối tới SMTP của Google và thực hiện gửi thư.
        /// Quản lý luôn cả logic tự động đổi tài khoản nếu đạt ngưỡng giới hạn.
        /// </summary>
        private async Task<bool> ProcessEmailJobAsync(EmailJob job)
        {
            // 1. Đọc các thông số cơ bản từ appsettings.json
            string smtpServer = _config["EmailSettings:SmtpServer"] ?? "";
            string smtpPort = _config["EmailSettings:SmtpPort"] ?? "587";
            string senderName = _config["EmailSettings:SenderName"] ?? "Hệ thống Quản lý";
            
            // Lấy giới hạn số lượng email cho 1 tài khoản (Google cho phép 500, ta đặt an toàn là 400)
            if (!int.TryParse(_config["EmailSettings:MaxEmailsPerAccount"], out int maxEmailsPerAccount))
            {
                maxEmailsPerAccount = 400; 
            }

            // Đọc mảng các tài khoản (Accounts array) từ file cấu hình
            var accounts = _config.GetSection("EmailSettings:Accounts").GetChildren().ToList();

            if (!accounts.Any())
            {
                _logger.LogWarning("[Email Background Sender] Không có tài khoản email nào được cấu hình trong appsettings!");
                return true; // Trả về true để bỏ qua (hủy) thư này, tránh lặp vô tận gây kẹt Queue
            }

            // 2. LOGIC LUÂN PHIÊN (ROTATION): Kiểm tra xem tài khoản hiện tại đã dùng hết lượt chưa
            if (_currentAccountSendCount >= maxEmailsPerAccount)
            {
                _currentAccountIndex++;             // Nhảy sang tài khoản tiếp theo
                _currentAccountSendCount = 0;       // Reset bộ đếm về 0 cho tài khoản mới
                _logger.LogWarning($"[Email Background Sender] Tài khoản đã đạt giới hạn ({maxEmailsPerAccount} thư). Tự động chuyển sang tài khoản dự phòng...");
            }

            // Nếu đã dùng hết toàn bộ tài khoản trong danh sách, quay ngược lại tài khoản số 1
            if (_currentAccountIndex >= accounts.Count)
            {
                _logger.LogCritical($"[Email Background Sender] CẢNH BÁO: Đã sử dụng HẾT sạch {accounts.Count} tài khoản! Bắt đầu quay vòng lại từ tài khoản đầu tiên.");
                _currentAccountIndex = 0;
                _currentAccountSendCount = 0;
            }

            // Lấy thông tin tài khoản đang được chọn để làm nhiệm vụ
            var activeAccount = accounts[_currentAccountIndex];
            string currentEmail = activeAccount["Email"];
            string currentPassword = activeAccount["AppPassword"];

            try
            {
                // 3. THIẾT LẬP KẾT NỐI VÀ GỬI
                using var client = new SmtpClient(smtpServer, int.Parse(smtpPort))
                {
                    Credentials = new NetworkCredential(currentEmail, currentPassword),
                    EnableSsl = true // Phải bật SSL để Google cho phép gửi
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(currentEmail, senderName),
                    Subject = job.Subject,
                    Body = job.Body,
                    IsBodyHtml = false // Gửi dưới dạng Text thuần (nếu muốn gửi HTML thì đổi thành true)
                };
                mailMessage.To.Add(job.ToEmail);

                // Thực thi gửi
                await client.SendMailAsync(mailMessage);
                
                // Nếu gửi thành công, tiến hành cộng biến đếm
                _currentAccountSendCount++;
                int remaining = maxEmailsPerAccount - _currentAccountSendCount;
                _logger.LogInformation($"[Email Background Sender] Đã gửi thành công tới {job.ToEmail} bằng {currentEmail}. (Còn lại: {remaining} lượt). Hàng đợi còn {_emailQueue.Count} thư.");
                
                return true; // Báo hiệu đã gửi thành công, rút lá thư này khỏi Queue vĩnh viễn
            }
            catch (Exception ex)
            {
                _logger.LogError($"[Email Background Sender] Lỗi gửi email bằng {currentEmail}. Lỗi: {ex.Message}");
                
                // Nếu lỗi phát sinh do bị Google chặn vì gửi quá nhiều (vượt Quota/Limit)
                // Ta chủ động ép bộ đếm của tài khoản này lên kịch trần (maxEmailsPerAccount).
                // Mục đích là để ở vòng lặp thư tiếp theo, hệ thống sẽ tự động vứt tài khoản này đi và lấy tài khoản khác.
                if (ex.Message.Contains("quota") || ex.Message.Contains("limit") || ex.Message.Contains("Too many"))
                {
                    _currentAccountSendCount = maxEmailsPerAccount; 
                }
                
                return false; // Báo hiệu gửi thất bại, yêu cầu hàm ExecuteAsync nhét lại thư vào Queue
            }
        }
    }
}