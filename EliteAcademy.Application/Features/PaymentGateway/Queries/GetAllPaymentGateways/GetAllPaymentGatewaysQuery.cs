using EliteAcademy.Application.DTOs.Payment;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.PaymentGateway.Queries.GetAllPaymentGateways;

public record GetAllPaymentGatewaysQuery : IRequest<Result<List<PaymentGatewayDto>>>;
