using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.User.Commands.AssignRole;

public class AssignRoleHandler : IRequestHandler<AssignRoleCommand, Result<bool>>
{
    private readonly IUserManager _userManager;

    public AssignRoleHandler(IUserManager userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<bool>> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return Result<bool>.Fail("User not found.");

        var existingRoles = await _userManager.GetRolesAsync(user);
        var removalResult = await _userManager.RemoveFromRoleAsync(user, existingRoles.FirstOrDefault()!);

        if (!removalResult.Succeeded)
            return Result<bool>.Fail(removalResult.Errors, "Failed to remove existing roles.");

        var addResult = await _userManager.AddToRoleAsync(user, request.RoleName);
        if (!addResult.Succeeded)
            return Result<bool>.Fail(addResult.Errors, "Failed to assign new role.");

        return Result<bool>.Ok(true, $"Role '{request.RoleName}' assigned successfully.");
    }
}
