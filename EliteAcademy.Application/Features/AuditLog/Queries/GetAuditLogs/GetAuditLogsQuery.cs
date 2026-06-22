using EliteAcademy.Application.DTOs.AuditLog;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.AuditLog.Queries.GetAuditLogs;

public record GetAuditLogsQuery(
    string? EntityFilter,
    string? ActionFilter,
    int Page,
    int PageSize) : IRequest<Result<List<AuditLogDto>>>;
