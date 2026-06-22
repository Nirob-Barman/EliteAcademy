using MediatR;

namespace EliteAcademy.Application.Features.AuditLog.Commands.LogAudit;

public record LogAuditCommand(
    string EntityName,
    string Action,
    string? Details,
    string? OldValues,
    string? NewValues) : IRequest<Unit>;
