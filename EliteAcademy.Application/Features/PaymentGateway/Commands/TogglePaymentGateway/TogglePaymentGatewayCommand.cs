using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.PaymentGateway.Commands.TogglePaymentGateway;

public record TogglePaymentGatewayCommand(int Id) : IRequest<Result<bool>>;
