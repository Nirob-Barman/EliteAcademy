using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.AuditLog;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.AuditLog.Queries.GetAuditLogs;

public class GetAuditLogsHandler : IRequestHandler<GetAuditLogsQuery, Result<List<AuditLogDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAuditLogsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<AuditLogDto>>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var all = await _context.AuditLogs.AsNoTracking().ToListAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.EntityFilter))
            all = all.Where(a => a.EntityName.Contains(request.EntityFilter, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!string.IsNullOrWhiteSpace(request.ActionFilter))
            all = all.Where(a => a.Action.Contains(request.ActionFilter, StringComparison.OrdinalIgnoreCase)).ToList();

        var dtos = all
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                EntityName = a.EntityName,
                Action = a.Action,
                UserId = a.UserId,
                UserName = a.UserName,
                Details = a.Details,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                CreatedAt = a.CreatedAt
            }).ToList();

        return Result<List<AuditLogDto>>.Ok(dtos);
    }
}
