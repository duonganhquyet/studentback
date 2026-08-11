namespace StudentAcademicManagement.Application.DTOs.Dashboards
{
    public class SuperAdminDashboardResponse
    {
        public int TotalSchools { get; set; }
        public int ActiveSchools { get; set; }
        public int TotalSchoolAdmins { get; set; }
        public int TotalSystemUsers { get; set; }
    }

    public class SchoolAdminDashboardResponse
    {
        public int TotalStudents { get; set; }
        public int StudyingStudents { get; set; }
        public int QuitOrSuspendedStudents { get; set; }
        public int PendingCccdRequests { get; set; }
        public int PendingDocuments { get; set; }
        public int PendingSpecialCategories { get; set; }
    }

    public class StudentDashboardResponse
    {
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string AcademicStatus { get; set; } = string.Empty;
        public string CccdVerificationStatus { get; set; } = string.Empty;
        public bool IsIdentityLocked { get; set; }
        public int UploadedDocumentsCount { get; set; }
        public int ApprovedSpecialCategoriesCount { get; set; }
        public int TotalPendingDocuments { get; set; }
        public int TotalPendingSpecialCategories { get; set; }
    }
}