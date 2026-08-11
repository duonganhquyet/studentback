namespace StudentAcademicManagement.Domain.Entities
{
    public class StudentContact : BaseEntity
    {
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public string? PhoneNumber { get; set; } // SĐT Sinh viên
        public string? Address { get; set; } // Địa chỉ thường trú
        public string? TemporaryAddress { get; set; } // Địa chỉ tạm trú (KT3/Phòng trọ)

        // Thông tin liên hệ khẩn cấp
        public string? GuardianName { get; set; }
        public string? GuardianPhoneNumber { get; set; }
        public string? GuardianRelationship { get; set; } // Cha, Mẹ, Anh/Chị...
    }
}