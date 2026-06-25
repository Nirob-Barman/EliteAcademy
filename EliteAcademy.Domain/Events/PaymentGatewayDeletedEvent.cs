using EliteAcademy.Domain.Common;

namespace EliteAcademy.Domain.Events;

public record PaymentGatewayDeletedEvent(string GatewayName, int GatewayId) : IDomainEvent;
