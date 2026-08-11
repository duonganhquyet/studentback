using Microsoft.AspNetCore.Http;
using StudentAcademicManagement.Application.Interfaces;
using System.Text.RegularExpressions;
using Tesseract;

namespace StudentAcademicManagement.Infrastructure.Services
{
    public class TesseractOcrService : IOcrService
    {
        private readonly string _tessDataPath;

        public TesseractOcrService()
        {
            // Thư mục chứa data ngôn ngữ (vie.traineddata)
            // Phải đảm bảo bạn đã tạo thư mục 'tessdata' ở thư mục gốc của project Api và chép file ngôn ngữ vào đó
            _tessDataPath = Path.Combine(Directory.GetCurrentDirectory(), "tessdata");
        }

        public async Task<OcrResult> ExtractCccdDataAsync(IFormFile frontImage)
        {
            if (frontImage == null || frontImage.Length == 0)
                throw new ArgumentException("Ảnh không hợp lệ");

            if (!Directory.Exists(_tessDataPath))
            {
                throw new InvalidOperationException($"Không tìm thấy thư mục ngôn ngữ Tesseract tại: {_tessDataPath}. Vui lòng tạo thư mục 'tessdata' và tải file 'vie.traineddata' vào.");
            }

            // Lưu file ảnh vào byte array
            using var ms = new MemoryStream();
            await frontImage.CopyToAsync(ms);
            byte[] imageBytes = ms.ToArray();

            try
            {
                // Khởi tạo Tesseract Engine với ngôn ngữ Tiếng Việt (vie)
                using var engine = new TesseractEngine(_tessDataPath, "vie", EngineMode.Default);
                using var img = Pix.LoadFromMemory(imageBytes);
                using var page = engine.Process(img);

                string extractedText = page.GetText();
                
                return ParseTextToOcrResult(extractedText);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Lỗi khởi tạo Tesseract (có thể thiếu file vie.traineddata hoặc lỗi file ảnh): " + ex.Message);
            }
        }

        private OcrResult ParseTextToOcrResult(string text)
        {
            var result = new OcrResult();
            if (string.IsNullOrWhiteSpace(text)) return result;

            var lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(l => l.Trim())
                            .Where(l => l.Length > 0)
                            .ToList();

            int idLineIndex = -1;

            // 1. Tìm ID Number
            for (int i = 0; i < lines.Count; i++)
            {
                var cleanedLine = lines[i].Replace("O", "0").Replace("o", "0")
                                          .Replace("I", "1").Replace("l", "1").Replace("|", "1")
                                          .Replace("B", "8").Replace("S", "5");
                
                var idMatch = Regex.Match(cleanedLine, @"\b(\d{12})\b");
                if (idMatch.Success)
                {
                    result.IdNumber = idMatch.Groups[1].Value;
                    idLineIndex = i;
                    break;
                }
            }

            // 2. Tên thường nằm ngay dưới ID
            if (idLineIndex >= 0)
            {
                for (int i = idLineIndex + 1; i <= Math.Min(idLineIndex + 3, lines.Count - 1); i++)
                {
                    var line = lines[i];
                    line = Regex.Replace(line, @"(?i)(Họ và tên|Full name|Ho va ten)[:\-]*", "").Trim();
                    
                    if (line.Length > 4 && Regex.Matches(line, @"[a-zA-ZÀ-ỹ]").Count > line.Length / 2)
                    {
                        result.FullName = line.ToUpper();
                        break;
                    }
                }
            }

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

            // 3. Tìm Ngày sinh
            foreach (var line in lines)
            {
                var dobMatch = Regex.Match(line, @"(\d{2})[^\d]{1,2}(\d{2})[^\d]{1,2}(\d{4})");
                if (dobMatch.Success)
                {
                    int.TryParse(dobMatch.Groups[1].Value, out int day);
                    int.TryParse(dobMatch.Groups[2].Value, out int month);
                    int.TryParse(dobMatch.Groups[3].Value, out int year);

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
