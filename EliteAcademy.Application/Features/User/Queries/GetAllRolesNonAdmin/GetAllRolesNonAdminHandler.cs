using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.User.Queries.GetAllRolesNonAdmin;

public class GetAllRolesNonAdminHandler : IRequestHandler<GetAllRolesNonAdminQuery, Result<List<string>>>
{
    private readonly IRoleManager _roleManager;

    public GetAllRolesNonAdminHandler(IRoleManager roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<Result<List<string>>> Handle(GetAllRolesNonAdminQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var roles = await _roleManager.GetAllRolesAsync(excludeAdmin: true);
            return Result<List<string>>.Ok(roles);
        }
        catch (Exception ex)
        {
            return Result<List<string>>.Fail("Failed to retrieve roles.", ex.Message);
        }
    }
}
