using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Domain.Events;
using MediatR;

namespace EliteAcademy.Application.Features.PaymentGateway.Events;

public class PaymentGatewayDeletedEventHandler : INotificationHandler<PaymentGatewayDeletedEvent>
{
    private readonly IAuditLogService _audit;

    public PaymentGatewayDeletedEventHandler(IAuditLogService audit) => _audit = audit;

    public async Task Handle(PaymentGatewayDeletedEvent notification, CancellationToken cancellationToken)
    {
        await _audit.LogAsync("PaymentGateway", "Delete",
            details: $"Deleted gateway \"{notification.GatewayName}\"");
    }
}
