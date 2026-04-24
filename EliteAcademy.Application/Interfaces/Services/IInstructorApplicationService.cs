using EliteAcademy.Application.DTOs.InstructorApplication;
using EliteAcademy.Application.Wrappers;

namespace EliteAcademy.Application.Interfaces.Services
{
    public interface IInstructorApplicationService
    {
        /// <summary>Submit a new application for the current user.</summary>
        Task<Result<InstructorApplicationDto>> ApplyAsync(InstructorApplicationFormDto dto);

        /// <summary>Return the current user's latest application, or null if none.</summary>
        Task<Result<InstructorApplicationDto?>> GetMyApplicationAsync();

        /// <summary>Admin: all applications, newest first.</summary>
        Task<Result<List<InstructorApplicationDto>>> GetAllAsync();

        /// <summary>Admin: pending applications only.</summary>
        Task<Result<List<InstructorApplicationDto>>> GetPendingAsync();

        /// <summary>Admin: approve an application → changes role to Instructor.</summary>
        Task<Result<bool>> ApproveAsync(int applicationId);

        /// <summary>Admin: reject an application with a reason.</summary>
        Task<Result<bool>> RejectAsync(int applicationId, string adminNotes);
    }
}
