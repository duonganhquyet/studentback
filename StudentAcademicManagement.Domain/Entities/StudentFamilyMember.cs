namespace StudentAcademicManagement.Domain.Entities
{
    public class StudentFamilyMember : BaseEntity
    {
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public string FullName { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty; // Cha, Mẹ, Anh, Chị...
        public string? Nationality { get; set; } // Quốc tịch
        public string? BirthYear { get; set; } // Năm sinh (hoặc tuổi)
        public string? Job { get; set; } // Nghề nghiệp
        public string? Position { get; set; } // Chức vụ
        public string? Company { get; set; } // Cơ quan công tác
        public string? Ethnicity { get; set; } // Dân tộc
        public string? Religion { get; set; } // Tôn giáo
        public string? PhoneNumber { get; set; } // Điện thoại
        public string? PermanentAddress { get; set; } // Hộ khẩu thường trú
        public string? CurrentAddress { get; set; } // Chỗ ở hiện tại
        public bool IsEmergencyContact { get; set; } = false; // Là người liên hệ khẩn cấp
        public bool IsAlumni { get; set; } = false; // Là cựu sinh viên trường
    }
}
