using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Payment.Commands.HandlePaymentSuccess;

public record HandlePaymentSuccessCommand(int TxId, string GatewaySlug, Dictionary<string, string> CallbackParams)
    : IRequest<Result<bool>>;
