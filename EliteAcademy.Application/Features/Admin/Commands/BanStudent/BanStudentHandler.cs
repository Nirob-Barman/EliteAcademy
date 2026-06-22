using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Admin.Commands.BanStudent;

public class BanStudentHandler : IRequestHandler<BanStudentCommand, Result<bool>>
{
    private readonly IUserManager _userManager;
    private readonly IAuditLogService _auditLogService;

    public BanStudentHandler(IUserManager userManager, IAuditLogService auditLogService)
    {
        _userManager = userManager;
        _auditLogService = auditLogService;
    }

    public async Task<Result<bool>> Handle(BanStudentCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.StudentId);
        if (user == null)
            return Result<bool>.Fail("Student not found.");

        var result = await _userManager.BanUserAsync(request.StudentId);
        if (!result.Succeeded)
            return Result<bool>.Fail(result.Errors.FirstOrDefault() ?? "Failed to ban student.");

        await _auditLogService.LogAsync("User", "Ban",
            details: $"Banned student {user.Email} (ID: {request.StudentId})");

        return Result<bool>.Ok(true, $"{user.Email} has been banned.");
    }
}
