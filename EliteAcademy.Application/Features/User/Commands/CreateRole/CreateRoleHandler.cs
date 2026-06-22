using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.User.Commands.CreateRole;

public class CreateRoleHandler : IRequestHandler<CreateRoleCommand, Result<bool>>
{
    private readonly IRoleManager _roleManager;

    public CreateRoleHandler(IRoleManager roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<Result<bool>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RoleName))
            return Result<bool>.Fail("Role name is required.");

        var roleName = request.RoleName.Trim();

        if (await _roleManager.RoleExistsAsync(roleName))
            return Result<bool>.Fail($"Role '{roleName}' already exists.");

        var result = await _roleManager.CreateRoleAsync(roleName);

        if (!result.Succeeded)
            return Result<bool>.Fail(result.Errors, "Failed to create role.");

        return Result<bool>.Ok(true, $"Role '{roleName}' created successfully.");
    }
}
