using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Admin.Commands.UnbanStudent;

public class UnbanStudentHandler : IRequestHandler<UnbanStudentCommand, Result<bool>>
{
    private readonly IUserManager _userManager;
    private readonly IAuditLogService _auditLogService;

    public UnbanStudentHandler(IUserManager userManager, IAuditLogService auditLogService)
    {
        _userManager = userManager;
        _auditLogService = auditLogService;
    }

    public async Task<Result<bool>> Handle(UnbanStudentCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.StudentId);
        if (user == null)
            return Result<bool>.Fail("Student not found.");

        var result = await _userManager.UnbanUserAsync(request.StudentId);
        if (!result.Succeeded)
            return Result<bool>.Fail(result.Errors.FirstOrDefault() ?? "Failed to unban student.");

        await _auditLogService.LogAsync("User", "Unban",
            details: $"Unbanned student {user.Email} (ID: {request.StudentId})");

        return Result<bool>.Ok(true, $"{user.Email} has been unbanned.");
    }
}
