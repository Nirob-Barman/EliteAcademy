using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Payment.Commands.InitiatePayment;

public record InitiatePaymentCommand(int PreEnrollmentId, string GatewaySlug, string BaseUrl)
    : IRequest<Result<string>>;
