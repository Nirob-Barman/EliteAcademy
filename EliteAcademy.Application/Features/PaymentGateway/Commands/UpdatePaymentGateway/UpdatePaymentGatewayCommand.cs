using EliteAcademy.Application.DTOs.Payment;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.PaymentGateway.Commands.UpdatePaymentGateway;

public record UpdatePaymentGatewayCommand(int Id, PaymentGatewayFormDto Dto) : IRequest<Result<bool>>;
