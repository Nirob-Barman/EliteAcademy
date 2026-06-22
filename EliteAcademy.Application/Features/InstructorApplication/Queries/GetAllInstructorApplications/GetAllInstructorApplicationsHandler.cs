using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.InstructorApplication;
using EliteAcademy.Application.Mappers;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.InstructorApplication.Queries.GetAllInstructorApplications;

public class GetAllInstructorApplicationsHandler : IRequestHandler<GetAllInstructorApplicationsQuery, Result<List<InstructorApplicationDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllInstructorApplicationsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<InstructorApplicationDto>>> Handle(GetAllInstructorApplicationsQuery request, CancellationToken cancellationToken)
    {
        var apps = (await _context.InstructorApplications.AsNoTracking().ToListAsync(cancellationToken))
            .OrderByDescending(a => a.CreatedAt)
            .Select(InstructorApplicationMapper.ToDto)
            .ToList();

        return Result<List<InstructorApplicationDto>>.Ok(apps);
    }
}
