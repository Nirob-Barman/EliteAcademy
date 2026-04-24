using EliteAcademy.Application.DTOs.Instructor;
using EliteAcademy.Application.Wrappers;

namespace EliteAcademy.Application.Interfaces.Services
{
    public interface IInstructorService
    {
        Task<Result<InstructorProfileDto>> GetProfileAsync();
        Task<Result<bool>> UpdateProfileAsync(InstructorProfileDto dto, Stream? imageStream, string? imageFileName);
        Task<Result<InstructorDashboardDto>>       GetDashboardAsync();
        Task<Result<List<InstructorProfileDto>>>   GetPublicInstructorListAsync();
        Task<Result<List<ClassStudentDto>>>        GetClassStudentsAsync(int classId);
    }
}
