using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Home;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Admin.Queries.GetPlatformStats;

public class GetPlatformStatsHandler : IRequestHandler<GetPlatformStatsQuery, Result<PlatformStatsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserManager _userManager;

    public GetPlatformStatsHandler(IApplicationDbContext context, IUserManager userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<Result<PlatformStatsDto>> Handle(GetPlatformStatsQuery request, CancellationToken cancellationToken)
    {
        var students = await _userManager.GetUsersByRoleAsync("Student");
        var instructors = await _userManager.GetUsersByRoleAsync("Instructor");
        var enrollments = await _context.Enrollments.CountAsync(cancellationToken);
        var classes = await _context.Classes.CountAsync(c => c.Status == ClassStatus.Approved, cancellationToken);

        return Result<PlatformStatsDto>.Ok(new PlatformStatsDto
        {
            ActiveStudents = students.Count(),
            ExpertInstructors = instructors.Count(),
            TotalEnrollments = enrollments,
            ApprovedClasses = classes
        });
    }
}
