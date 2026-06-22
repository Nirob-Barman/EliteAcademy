using EliteAcademy.Application.DTOs.Identity;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.User.Queries.GetMyProfile;

public class GetMyProfileHandler : IRequestHandler<GetMyProfileQuery, Result<EditProfileDto>>
{
    private readonly IUserManager _userManager;
    private readonly IUserContextService _userContextService;

    public GetMyProfileHandler(IUserManager userManager, IUserContextService userContextService)
    {
        _userManager = userManager;
        _userContextService = userContextService;
    }

    public async Task<Result<EditProfileDto>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(_userContextService.UserId!);
        if (user == null) return Result<EditProfileDto>.Fail("User not found.");

        var roles = await _userManager.GetRolesAsync(user);

        return Result<EditProfileDto>.Ok(new EditProfileDto
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Gender = user.Gender,
            DateOfBirth = user.DateOfBirth,
            Address = user.Address,
            ImageUrl = user.ImageUrl,
            Role = roles.FirstOrDefault()
        });
    }
}
