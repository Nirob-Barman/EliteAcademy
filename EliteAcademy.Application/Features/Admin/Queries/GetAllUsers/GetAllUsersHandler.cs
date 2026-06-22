using EliteAcademy.Application.DTOs.Admin;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Admin.Queries.GetAllUsers;

public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, Result<List<AdminUserDto>>>
{
    private readonly IUserManager _userManager;

    public GetAllUsersHandler(IUserManager userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<List<AdminUserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userManager.GetAllUsersAsync();
        var dtos = new List<AdminUserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            dtos.Add(new AdminUserDto
            {
                Id = user.Id,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Email = user.Email,
                Role = roles.FirstOrDefault() ?? "No Role"
            });
        }

        return Result<List<AdminUserDto>>.Ok(dtos);
    }
}
