using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.InstructorApplication;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Mappers;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.InstructorApplication.Queries.GetMyInstructorApplication;

public class GetMyInstructorApplicationHandler : IRequestHandler<GetMyInstructorApplicationQuery, Result<InstructorApplicationDto?>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;

    public GetMyInstructorApplicationHandler(IApplicationDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<Result<InstructorApplicationDto?>> Handle(GetMyInstructorApplicationQuery request, CancellationToken cancellationToken)
    {
        var userId = _userContextService.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Result<InstructorApplicationDto?>.Ok(null);

        var apps = await _context.InstructorApplications.AsNoTracking()
            .Where(a => a.ApplicantId == userId)
            .ToListAsync(cancellationToken);

        var latest = apps.OrderByDescending(a => a.CreatedAt).FirstOrDefault();

        return Result<InstructorApplicationDto?>.Ok(
            latest != null ? InstructorApplicationMapper.ToDto(latest) : null);
    }
}
