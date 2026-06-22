using EliteAcademy.Application.DTOs.Payment;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.PaymentGateway.Commands.CreatePaymentGateway;

public record CreatePaymentGatewayCommand(PaymentGatewayFormDto Dto) : IRequest<Result<bool>>;
