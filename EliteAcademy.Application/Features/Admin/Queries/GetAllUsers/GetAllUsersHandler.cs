using EliteAcademy.Application.DTOs.Admin;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Admin.Queries.GetAllUsers;

public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, Result<PagedResult<AdminUserDto>>>
{
    private readonly IUserManager _userManager;

    public GetAllUsersHandler(IUserManager userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<PagedResult<AdminUserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var allUsers = (await _userManager.GetAllUsersAsync()).ToList();
        var total = allUsers.Count;
        var pageUsers = allUsers
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var dtos = new List<AdminUserDto>();
        foreach (var user in pageUsers)
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

        return Result<PagedResult<AdminUserDto>>.Ok(new PagedResult<AdminUserDto>
        {
            Items = dtos,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}
