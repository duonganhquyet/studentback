namespace StudentAcademicManagement.Domain.Entities
{
    public class SpecialCategory : BaseEntity
    {
        public int SchoolId { get; set; }
        public School School { get; set; } = null!;

        public string Name { get; set; } = string.Empty; // VD: Hộ nghèo, Gia đình chính sách...
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}