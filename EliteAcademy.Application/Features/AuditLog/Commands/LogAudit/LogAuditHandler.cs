using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using MediatR;
using AuditLogEntity = EliteAcademy.Domain.Entities.AuditLog;

namespace EliteAcademy.Application.Features.AuditLog.Commands.LogAudit;

public class LogAuditHandler : IRequestHandler<LogAuditCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;

    public LogAuditHandler(IApplicationDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<Unit> Handle(LogAuditCommand request, CancellationToken cancellationToken)
    {
        _context.AuditLogs.Add(new AuditLogEntity
        {
            EntityName = request.EntityName,
            Action = request.Action,
            UserId = _userContextService.UserId,
            UserName = _userContextService.Email,
            Details = request.Details,
            OldValues = request.OldValues,
            NewValues = request.NewValues,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
