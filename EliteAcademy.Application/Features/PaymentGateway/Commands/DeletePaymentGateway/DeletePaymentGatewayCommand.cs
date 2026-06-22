using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.PaymentGateway.Commands.DeletePaymentGateway;

public record DeletePaymentGatewayCommand(int Id) : IRequest<Result<bool>>;
