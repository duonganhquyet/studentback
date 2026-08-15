namespace StudentAcademicManagement.Domain.Entities
{
    public class StudentProfile : BaseEntity
    {
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public string FullName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; } // Nam, Nữ, Khác
        public string? Ethnicity { get; set; } // Dân tộc
        public string? Nationality { get; set; } // Quốc tịch
        public string? PlaceOfBirth { get; set; } // Nơi sinh
        public string? AvatarUrl { get; set; }
        public string? RegionType { get; set; } // Khu vực (KV1, KV2, KV2-NT, KV3)

        // Các hoạt động tham gia
        public bool HasBeenClassMonitor { get; set; }
        public bool HasBeenYouthUnionOfficer { get; set; }
        public bool HasParticipatedInExcellentStudentTeam { get; set; }
        public string? AwardDetails { get; set; }

        // Trường hiện đang làm do Admin cấp
        public string? CurrentRoleInSchool { get; set; } // VD: "Lớp trưởng lớp", "Trợ giảng"
    }
}