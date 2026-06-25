using EliteAcademy.Domain.Common;

namespace EliteAcademy.Domain.Events;

public record CouponDeletedEvent(string CouponCode, int CouponId) : IDomainEvent;
