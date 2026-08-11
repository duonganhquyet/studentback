using StudentAcademicManagement.Application.DTOs.Students;

namespace StudentAcademicManagement.Application.Interfaces
{
	public interface IStudentContactService
	{
		Task<StudentContactResponse> GetContactByStudentIdAsync(int schoolId, int studentId);
		Task<StudentContactResponse> UpdateContactAsync(int schoolId, int studentId, UpdateStudentContactRequest request);
	}
}