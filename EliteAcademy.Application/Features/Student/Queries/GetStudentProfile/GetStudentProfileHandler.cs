using EliteAcademy.Application.DTOs.Student;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Student.Queries.GetStudentProfile;

public class GetStudentProfileHandler : IRequestHandler<GetStudentProfileQuery, Result<StudentProfileDto>>
{
    private readonly IUserManager _userManager;
    private readonly IUserContextService _userContextService;

    public GetStudentProfileHandler(IUserManager userManager, IUserContextService userContextService)
    {
        _userManager = userManager;
        _userContextService = userContextService;
    }

    public async Task<Result<StudentProfileDto>> Handle(GetStudentProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(_userContextService.UserId!);
        if (user == null)
            return Result<StudentProfileDto>.Fail("User not found.");

        return Result<StudentProfileDto>.Ok(new StudentProfileDto
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            ImageUrl = user.ImageUrl
        });
    }
}
