using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.InstructorApplication;
using EliteAcademy.Application.Mappers;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.InstructorApplication.Queries.GetPendingInstructorApplications;

public class GetPendingInstructorApplicationsHandler : IRequestHandler<GetPendingInstructorApplicationsQuery, Result<List<InstructorApplicationDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetPendingInstructorApplicationsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<InstructorApplicationDto>>> Handle(GetPendingInstructorApplicationsQuery request, CancellationToken cancellationToken)
    {
        var apps = (await _context.InstructorApplications.AsNoTracking()
            .Where(a => a.Status == InstructorApplicationStatus.Pending)
            .ToListAsync(cancellationToken))
            .OrderBy(a => a.CreatedAt)
            .Select(InstructorApplicationMapper.ToDto)
            .ToList();

        return Result<List<InstructorApplicationDto>>.Ok(apps);
    }
}
