namespace StudentAcademicManagement.Application.DTOs.Students
{
    public class AcademicHistoryDto
    {
        public int Id { get; set; }
        public string? FromTime { get; set; }
        public string? ToTime { get; set; }
        public string? SchoolName { get; set; }
        public string? Notes { get; set; }
    }
}
