using EliteAcademy.Application.DTOs.Student;
using EliteAcademy.Application.Wrappers;

namespace EliteAcademy.Application.Interfaces.Services
{
    public interface IStudentService
    {
        Task<Result<StudentDashboardDto>>                              GetDashboardAsync();
        Task<Result<List<PreEnrollmentDto>>>                          GetSelectedClassesAsync();
        Task<Result<bool>>                                            SelectClassAsync(int classId);
        Task<Result<bool>>                                            DeleteSelectedClassAsync(int preEnrollmentId);
        Task<Result<bool>>                                            PayForClassAsync(int preEnrollmentId);
        Task<Result<List<EnrollmentDto>>>                             GetEnrolledClassesAsync();
        Task<Result<(HashSet<int> Selected, HashSet<int> Enrolled)>> GetEnrollmentStatusAsync();
        Task<Result<StudentProfileDto>>                               GetProfileAsync();
        Task<Result<bool>>                                            UpdateProfileAsync(StudentProfileDto dto, Stream? imageStream, string? imageFileName);
        Task<Result<bool>>                                            ApplyCouponAsync(int preEnrollmentId, string couponCode);
        Task<Result<bool>>                                            RemoveCouponAsync(int preEnrollmentId);
    }
}
