using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace StudentAcademicManagement.Application.DTOs.Schools
{
	public class UpdateSchoolRequest
	{
		[Required(ErrorMessage = "Tên trường là bắt buộc")]
		public string SchoolName { get; set; } = string.Empty;

		public string? ShortName { get; set; }

		[EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
		public string? Email { get; set; }

		public string? PhoneNumber { get; set; }
		public string? Address { get; set; }
		public string? Website { get; set; }
		public string? Description { get; set; }

		// Cho phép cập nhật Logo mới. Nếu null, giữ nguyên logo cũ.
		public IFormFile? Logo { get; set; }
	}
}