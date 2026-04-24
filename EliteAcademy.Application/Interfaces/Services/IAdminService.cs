using EliteAcademy.Application.DTOs.Admin;
using EliteAcademy.Application.DTOs.Class;
using EliteAcademy.Application.DTOs.Home;
using EliteAcademy.Application.Wrappers;

namespace EliteAcademy.Application.Interfaces.Services
{
    public interface IAdminService
    {
        Task<Result<AdminDashboardDto>>  GetDashboardAsync();
        Task<Result<List<AdminUserDto>>> GetAllUsersAsync();
        Task<Result<bool>>               ChangeUserRoleAsync(string userId, string newRole);
        Task<Result<List<ClassDto>>>     GetAllClassesAsync();
        Task<Result<bool>>               ApproveClassAsync(int classId);
        Task<Result<bool>>               RejectClassAsync(int classId, string feedback);
        Task<Result<PlatformStatsDto>>   GetPlatformStatsAsync();

        // Student management
        Task<Result<List<AdminStudentDto>>> GetAllStudentsAsync();
        Task<Result<bool>>                  BanStudentAsync(string studentId);
        Task<Result<bool>>                  UnbanStudentAsync(string studentId);

        // Class enrollments
        Task<Result<AdminClassEnrollmentsDto>> GetClassEnrollmentsAsync(int classId);

        // Revenue reports
        Task<Result<RevenueReportDto>> GetRevenueReportAsync(int year);
    }
}
