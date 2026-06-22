using EliteAcademy.Application.DTOs.Instructor;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Mappers;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Instructor.Queries.GetInstructorProfile;

public class GetInstructorProfileHandler : IRequestHandler<GetInstructorProfileQuery, Result<InstructorProfileDto>>
{
    private readonly IUserManager _userManager;
    private readonly IUserContextService _userContextService;

    public GetInstructorProfileHandler(IUserManager userManager, IUserContextService userContextService)
    {
        _userManager        = userManager;
        _userContextService = userContextService;
    }

    public async Task<Result<InstructorProfileDto>> Handle(GetInstructorProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(_userContextService.UserId!);
        if (user == null)
            return Result<InstructorProfileDto>.Fail("User not found.");

        return Result<InstructorProfileDto>.Ok(InstructorMapper.ToProfileDto(user));
    }
}
