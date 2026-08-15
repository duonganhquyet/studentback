using System;
using System.Collections.Generic;

namespace StudentAcademicManagement.Application.DTOs.Students
{
    public class CreatePaperRequest
    {
        public string PaperType { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    public class PaperRequestResponse
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PaperType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> SpecialCategories { get; set; } = new List<string>();
        public string AcademicStatus { get; set; } = string.Empty;
    }

    public class ReviewPaperRequest
    {
        public string Status { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
    }
}
