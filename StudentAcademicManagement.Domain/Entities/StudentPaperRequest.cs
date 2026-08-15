using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentAcademicManagement.Domain.Entities
{
    public class StudentPaperRequest : BaseEntity
    {
        public int StudentId { get; set; }
        public Student Student { get; set; }

        [Required]
        [MaxLength(255)]
        public string PaperType { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        [MaxLength(500)]
        public string? Note { get; set; }
        
        [MaxLength(500)]
        public string? RejectionReason { get; set; }
    }
}
