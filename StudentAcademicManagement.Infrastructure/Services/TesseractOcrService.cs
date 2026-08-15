using Microsoft.AspNetCore.Http;
using StudentAcademicManagement.Application.Interfaces;
using System.Text.RegularExpressions;
using Tesseract;
using System.IO;
using System.Threading.Tasks;
using System;

namespace StudentAcademicManagement.Infrastructure.Services
{
    // ==============================================================================
    // DỊCH VỤ TRÍCH XUẤT CHỮ TRÊN ẢNH (OCR SERVICE DÙNG TESSERACT)
    // ==============================================================================
    /// <summary>
    /// Sử dụng thư viện mã nguồn mở Tesseract của Google để nhận diện chữ trên ảnh CCCD.
    /// Dịch vụ này đọc file ảnh, dùng bộ từ điển Tiếng Việt (vie.traineddata) để phân tách điểm ảnh thành văn bản.
    /// Sau đó dùng Regex (Biểu thức chính quy) để tìm đúng các trường (Tên, CCCD, Quê quán...).
    /// </summary>
    public class TesseractOcrService : IOcrService
    {
        private readonly string _tessDataPath;

        public TesseractOcrService()
        {
            // Thư mục chứa data ngôn ngữ (vie.traineddata). Bắt buộc phải có để AI có thể hiểu chữ tiếng Việt.
            _tessDataPath = Path.Combine(Directory.GetCurrentDirectory(), "tessdata");
        }

        public async Task<OcrResult> ExtractCccdDataAsync(IFormFile frontImage)
        {
            // Kiểm tra tính hợp lệ của ảnh đầu vào
            if (frontImage == null || frontImage.Length == 0)
                throw new ArgumentException("Ảnh không hợp lệ");

            if (!Directory.Exists(_tessDataPath))
            {
                throw new InvalidOperationException($"Không tìm thấy thư mục ngôn ngữ Tesseract tại: {_tessDataPath}. Vui lòng tạo thư mục 'tessdata' và tải file 'vie.traineddata' vào.");
            }

            // Lưu file ảnh vào vùng nhớ đệm (MemoryStream) và chuyển hóa thành mảng Byte (byte array) để đưa cho Tesseract đọc
            using var ms = new MemoryStream();
            await frontImage.CopyToAsync(ms);
            byte[] imageBytes = ms.ToArray();

            try
            {
                // Khởi tạo Tesseract Engine. Cờ "vie" báo cho AI biết đây là Tiếng Việt.
                using var engine = new TesseractEngine(_tessDataPath, "vie", EngineMode.Default);
                
                // Nạp bức ảnh vào bộ giải mã
                using var img = Pix.LoadFromMemory(imageBytes);
                
                // Bắt đầu quá trình quét qua tất cả các điểm ảnh (Processing)
                using var page = engine.Process(img);

                // Trích xuất toàn bộ văn bản thô (một mớ lộn xộn các chữ cái) ra string
                string extractedText = page.GetText();
                
                // Đẩy mớ chữ thô này sang hàm phân tích để bóc tách Tên, CCCD, Ngày sinh...
                return ParseTextToOcrResult(extractedText);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Lỗi khởi tạo Tesseract (có thể thiếu file vie.traineddata hoặc lỗi file ảnh): " + ex.Message);
            }
        }

        // ==============================================================================
        // HÀM PHÂN TÍCH VĂN BẢN (PARSER)
        // ==============================================================================
        /// <summary>
        /// Đây là trái tim của việc trích xuất dữ liệu. 
        /// Nó nhận vào 1 đoạn văn bản thô (ví dụ: "CỘNG HÒA XÃ HỘI... Số: 038204... Họ và tên: NGUYỄN VĂN A")
        /// Và dùng các "Biểu thức chính quy" (Regex) để truy tìm từng mảnh thông tin chính xác.
        /// </summary>
        private OcrResult ParseTextToOcrResult(string text)
        {
            var result = new OcrResult();
            if (string.IsNullOrWhiteSpace(text)) return result;

            // Tách đoạn văn bản thô thành một mảng các dòng, bỏ đi các dòng trống
            var lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(l => l.Trim())
                            .Where(l => l.Length > 0)
                            .ToList();

            int idLineIndex = -1;

            // 1. Tìm Số CCCD (ID Number)
            for (int i = 0; i < lines.Count; i++)
            {
                // Tiền xử lý: Tesseract đôi khi nhìn nhầm các chữ cái giống số.
                // Ta thay thế thủ công O -> 0, I -> 1, B -> 8... để tăng độ chính xác.
                var cleanedLine = lines[i].Replace("O", "0").Replace("o", "0")
                                          .Replace("I", "1").Replace("l", "1").Replace("|", "1")
                                          .Replace("B", "8").Replace("S", "5");
                
                // Regex tìm chuỗi có đúng 12 chữ số liên tiếp
                var idMatch = Regex.Match(cleanedLine, @"\b(\d{12})\b");
                if (idMatch.Success)
                {
                    result.IdNumber = idMatch.Groups[1].Value;
                    idLineIndex = i; // Lưu lại vị trí dòng chứa CCCD để tìm Tên ở ngay bên dưới
                    break;
                }
            }

            // 2. Tìm Họ và Tên (FullName) - Tên thường nằm ngay dưới số CCCD 1-3 dòng
            if (idLineIndex >= 0)
            {
                for (int i = idLineIndex + 1; i <= Math.Min(idLineIndex + 3, lines.Count - 1); i++)
                {
                    var line = lines[i];
                    
                    // Cắt bỏ chữ "Họ và tên:" để chỉ lấy phần tên thực sự
                    line = Regex.Replace(line, @"(?i)(Họ và tên|Full name|Ho va ten)[:\-]*", "").Trim();
                    
                    // Nếu dòng này dài hơn 4 ký tự và phần lớn là chữ cái (không phải số/rác) thì khả năng cao là Tên
                    if (line.Length > 4 && Regex.Matches(line, @"[a-zA-ZÀ-ỹ]").Count > line.Length / 2)
                    {
                        result.FullName = line.ToUpper(); // Chuẩn hóa in hoa
                        break;
                    }
                }
            }

            // Cơ chế dự phòng tìm tên: Quét toàn bộ văn bản tìm dòng nào toàn CHỮ IN HOA
            if (string.IsNullOrEmpty(result.FullName))
            {
                foreach (var line in lines)
                {
                    var cleanLine = Regex.Replace(line, @"(?i)(Họ và tên|Full name|Ho va ten)[:\-]*", "").Trim();
                    if (cleanLine.Length > 5 && Regex.IsMatch(cleanLine, @"^[A-ZÀ-Ỹ\s\.\,]+$"))
                    {
                        result.FullName = cleanLine.Replace(".", "").Replace(",", "").Trim();
                        break;
                    }
                }
            }

            // 3. Tìm Ngày sinh (Date of Birth)
            foreach (var line in lines)
            {
                // Regex tìm dạng Ngày-Tháng-Năm (ngăn cách bằng ký tự bất kỳ như /, -, .)
                var dobMatch = Regex.Match(line, @"(\d{2})[^\d]{1,2}(\d{2})[^\d]{1,2}(\d{4})");
                if (dobMatch.Success)
                {
                    int.TryParse(dobMatch.Groups[1].Value, out int day);
                    int.TryParse(dobMatch.Groups[2].Value, out int month);
                    int.TryParse(dobMatch.Groups[3].Value, out int year);

                    // Kiểm tra tính hợp lệ của ngày tháng (tránh nhận diện sai số khác)
                    if (day >= 1 && day <= 31 && month >= 1 && month <= 12 && year > 1900 && year <= DateTime.Now.Year)
                    {
                        try
                        {
                            result.DateOfBirth = new DateTime(year, month, day);
                            break;
                        }
                        catch { }
                    }
                }
            }

            // 4. Giới tính
            var genderLine = lines.FirstOrDefault(l => l.Contains("Nam") || l.Contains("Nữ") || l.Contains("Sex"));
            if (genderLine != null)
            {
                if (genderLine.Contains("Nam")) result.Gender = "Nam";
                else if (genderLine.Contains("Nữ")) result.Gender = "Nữ";
            }
            if (string.IsNullOrEmpty(result.Gender) && text.Contains("Nam")) result.Gender = "Nam";
            else if (string.IsNullOrEmpty(result.Gender) && text.Contains("Nữ")) result.Gender = "Nữ";

            // 5. Nơi thường trú / Quê quán
            int originIndex = lines.FindIndex(l => l.ToLower().Contains("quê quán") || l.ToLower().Contains("place of origin"));
            if (originIndex >= 0 && originIndex + 1 < lines.Count)
            {
                result.PlaceOfOrigin = lines[originIndex + 1].Trim();
            }

            int resIndex = lines.FindIndex(l => l.ToLower().Contains("thường trú") || l.ToLower().Contains("place of residence"));
            if (resIndex >= 0 && resIndex + 1 < lines.Count)
            {
                result.PlaceOfResidence = lines[resIndex + 1].Trim();
            }

            // Fallback
            if (string.IsNullOrEmpty(result.FullName)) result.FullName = "CẦN SỬA LẠI TÊN BẰNG TAY";
            if (string.IsNullOrEmpty(result.PlaceOfOrigin)) result.PlaceOfOrigin = "OCR chưa đọc rõ - Vui lòng nhập lại";
            if (string.IsNullOrEmpty(result.PlaceOfResidence)) result.PlaceOfResidence = "OCR chưa đọc rõ - Vui lòng nhập lại";

            return result;
        }
    }
}
