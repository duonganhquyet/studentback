namespace StudentAcademicManagement.Domain.Entities
{
    public class Student : BaseEntity
    {
        public string StudentCode { get; set; } = string.Empty;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int SchoolId { get; set; }
        public School School { get; set; } = null!;

        // Trạng thái học vụ của sinh viên (Mặc định khi tạo là Studying)
        // Các giá trị: Studying, Reserved (Bảo lưu), Suspended (Đình chỉ), Quit (Thôi học), Graduated (Tốt nghiệp)
        public string AcademicStatus { get; set; } = "Studying";

        // Thuộc tính mới: Khoa, Ngành, Lớp quản lý
        public string FacultyName { get; set; } = string.Empty; // Khoa
        public string MajorName { get; set; } = string.Empty;   // Ngành
        public string ClassName { get; set; } = string.Empty;   // Lớp quản lý

        public StudentProfile? Profile { get; set; }
        public StudentContact? Contact { get; set; }
        public StudentIdentity? Identity { get; set; }
    }
}