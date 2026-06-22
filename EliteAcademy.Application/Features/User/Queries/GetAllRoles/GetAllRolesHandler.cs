using EliteAcademy.Application.Interfaces.Identity;
using MediatR;

namespace EliteAcademy.Application.Features.User.Queries.GetAllRoles;

public class GetAllRolesHandler : IRequestHandler<GetAllRolesQuery, List<string>>
{
    private readonly IRoleManager _roleManager;

    public GetAllRolesHandler(IRoleManager roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<List<string>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        return await _roleManager.GetAllRolesAsync();
    }
}
