namespace StudentAcademicManagement.Domain.Entities
{
    public class StudentIdentity : BaseEntity
    {
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public string? IdNumber { get; set; } // Số CCCD
        public string? FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Nationality { get; set; }
        public string? PlaceOfOrigin { get; set; } // Quê quán
        public string? PlaceOfResidence { get; set; } // Thường trú
        public DateTime? IssueDate { get; set; }
        public string? IssuePlace { get; set; }

        public string? FrontImageUrl { get; set; }
        public string? BackImageUrl { get; set; }

        // Unverified (Chưa có), Pending (Đã upload, chờ duyệt/xác nhận), Verified (Đã xác thực)
        public string VerificationStatus { get; set; } = "Unverified";

        // Cờ khóa dữ liệu: Nếu true, hồ sơ Profile sẽ không được tự do sửa nữa.
        public bool IsLocked { get; set; } = false;
    }
}