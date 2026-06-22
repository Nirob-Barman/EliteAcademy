using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Admin.Commands.ChangeUserRole;

public class ChangeUserRoleHandler : IRequestHandler<ChangeUserRoleCommand, Result<bool>>
{
    private readonly IUserManager _userManager;
    private readonly IAuditLogService _auditLogService;

    public ChangeUserRoleHandler(IUserManager userManager, IAuditLogService auditLogService)
    {
        _userManager = userManager;
        _auditLogService = auditLogService;
    }

    public async Task<Result<bool>> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
    {
        var validRoles = new[] { "Admin", "Instructor", "Student" };
        if (!validRoles.Contains(request.NewRole))
            return Result<bool>.Fail("Invalid role.");

        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return Result<bool>.Fail("User not found.");

        var currentRoles = await _userManager.GetRolesAsync(user);
        var oldRole = currentRoles.FirstOrDefault() ?? "None";

        foreach (var role in currentRoles)
        {
            var removeResult = await _userManager.RemoveFromRoleAsync(user, role);
            if (!removeResult.Succeeded)
                return Result<bool>.Fail(removeResult.Errors.FirstOrDefault() ?? "Failed to remove existing role.");
        }

        var addResult = await _userManager.AddToRoleAsync(user, request.NewRole);
        if (!addResult.Succeeded)
            return Result<bool>.Fail(addResult.Errors.FirstOrDefault() ?? "Failed to assign new role.");

        await _auditLogService.LogAsync("User", "ChangeRole",
            details: $"Changed role for {user.Email} from {oldRole} to {request.NewRole}");

        return Result<bool>.Ok(true, $"Role changed to {request.NewRole}.");
    }
}
