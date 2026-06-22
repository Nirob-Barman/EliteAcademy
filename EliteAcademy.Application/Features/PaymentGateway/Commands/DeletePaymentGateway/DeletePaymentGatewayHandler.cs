using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.PaymentGateway.Commands.DeletePaymentGateway;

public class DeletePaymentGatewayHandler : IRequestHandler<DeletePaymentGatewayCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public DeletePaymentGatewayHandler(IApplicationDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<Result<bool>> Handle(DeletePaymentGatewayCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.PaymentGateways
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
        if (entity == null)
            return Result<bool>.Fail("Gateway not found.");

        var hasTx = await _context.PaymentTransactions.AnyAsync(t => t.GatewayId == request.Id, cancellationToken);
        if (hasTx)
            return Result<bool>.Fail("Cannot delete a gateway that has transactions.");

        _context.PaymentGateways.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync("PaymentGateway", "Delete",
            details: $"Deleted gateway \"{entity.Name}\"");

        return Result<bool>.Ok(true, "Payment gateway deleted.");
    }
}
