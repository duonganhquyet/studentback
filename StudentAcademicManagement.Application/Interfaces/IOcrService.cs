using Microsoft.AspNetCore.Http;

namespace StudentAcademicManagement.Application.Interfaces
{
    public class OcrResult
    {
        public string IdNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string PlaceOfOrigin { get; set; } = string.Empty;
        public string PlaceOfResidence { get; set; } = string.Empty;
        public DateTime? IssueDate { get; set; }
        public string? IssuePlace { get; set; }
    }

    public interface IOcrService
    {
        Task<OcrResult> ExtractCccdDataAsync(IFormFile frontImage);
    }
}