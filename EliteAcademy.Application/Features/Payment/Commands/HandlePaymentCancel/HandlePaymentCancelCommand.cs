using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Payment.Commands.HandlePaymentCancel;

public record HandlePaymentCancelCommand(int TxId) : IRequest<Result<bool>>;
