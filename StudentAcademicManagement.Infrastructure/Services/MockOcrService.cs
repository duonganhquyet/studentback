using Microsoft.AspNetCore.Http;
using StudentAcademicManagement.Application.Interfaces;

namespace StudentAcademicManagement.Infrastructure.Services
{
    public class MockOcrService : IOcrService
    {
        public async Task<OcrResult> ExtractCccdDataAsync(IFormFile frontImage)
        {
            // Giả lập delay của việc đọc ảnh AI
            await Task.Delay(1500);

            var random = new Random();
            return new OcrResult
            {
                IdNumber = "038204" + random.Next(100000, 999999).ToString(),
                FullName = "NGUYỄN VĂN A",
                DateOfBirth = new DateTime(2004, 11, 28),
                Gender = "Nam",
                PlaceOfOrigin = "Thôn 9, Thiệu Hóa, Thanh Hóa",
                PlaceOfResidence = "Phường Hàm Rồng, TP Thanh Hóa",
                IssueDate = new DateTime(2021, 8, 11),
                IssuePlace = "Cục Cảnh sát Quản lý hành chính về trật tự xã hội"
            };
        }
    }
}