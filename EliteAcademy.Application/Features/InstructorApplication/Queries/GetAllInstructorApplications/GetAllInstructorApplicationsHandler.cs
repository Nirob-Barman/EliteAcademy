using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.InstructorApplication;
using EliteAcademy.Application.Mappers;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.InstructorApplication.Queries.GetAllInstructorApplications;

public class GetAllInstructorApplicationsHandler : IRequestHandler<GetAllInstructorApplicationsQuery, Result<PagedResult<InstructorApplicationDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllInstructorApplicationsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<InstructorApplicationDto>>> Handle(GetAllInstructorApplicationsQuery request, CancellationToken cancellationToken)
    {
        var total = await _context.InstructorApplications.CountAsync(cancellationToken);

        var statusCounts = await _context.InstructorApplications
            .AsNoTracking()
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var apps = await _context.InstructorApplications
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = apps.Select(InstructorApplicationMapper.ToDto).ToList();

        return Result<PagedResult<InstructorApplicationDto>>.Ok(new PagedResult<InstructorApplicationDto>
        {
            Items = dtos,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize,
            StatusCounts = statusCounts.ToDictionary(s => s.Status.ToString(), s => s.Count)
        });
    }
}
