using System.ComponentModel.DataAnnotations;

namespace StudentAcademicManagement.Application.DTOs.Students
{
    public class UpdateStudentContactRequest
    {
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? TemporaryAddress { get; set; }

        public string? GuardianName { get; set; }

        [Phone(ErrorMessage = "Số điện thoại người giám hộ không hợp lệ")]
        public string? GuardianPhoneNumber { get; set; }
        public string? GuardianRelationship { get; set; }
    }
}