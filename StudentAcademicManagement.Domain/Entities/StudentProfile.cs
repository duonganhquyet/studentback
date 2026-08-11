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
    }
}