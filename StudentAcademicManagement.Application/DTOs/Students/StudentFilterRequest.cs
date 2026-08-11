namespace StudentAcademicManagement.Application.DTOs.Students
{
    public class StudentFilterRequest
    {
        public string? SearchTerm { get; set; } // Tìm theo MSSV hoặc Email
        public bool? IsActive { get; set; } // Lọc theo trạng thái tài khoản
        public string? FacultyName { get; set; } // Lọc theo Khoa
        public string? MajorName { get; set; }   // Lọc theo Ngành
        public string? ClassName { get; set; }   // Lọc theo Lớp
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}