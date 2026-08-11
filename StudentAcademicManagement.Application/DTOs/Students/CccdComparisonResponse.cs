using StudentAcademicManagement.Application.Interfaces;

namespace StudentAcademicManagement.Application.DTOs.Students
{
    public class CccdComparisonResponse
    {
        public OcrResult OcrData { get; set; } = null!;
        public StudentProfileResponse CurrentProfile { get; set; } = null!;
        public string FrontImageUrl { get; set; } = string.Empty;
        public string BackImageUrl { get; set; } = string.Empty;
    }
}