using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Domain.Events;
using MediatR;

namespace EliteAcademy.Application.Features.Coupon.Events;

public class CouponDeletedEventHandler : INotificationHandler<CouponDeletedEvent>
{
    private readonly IAuditLogService _audit;

    public CouponDeletedEventHandler(IAuditLogService audit) => _audit = audit;

    public async Task Handle(CouponDeletedEvent notification, CancellationToken cancellationToken)
    {
        await _audit.LogAsync("Coupon", "Delete",
            details: $"Deleted coupon \"{notification.CouponCode}\" (ID: {notification.CouponId})");
    }
}
