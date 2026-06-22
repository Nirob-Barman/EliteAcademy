using EliteAcademy.Application.DTOs.Payment;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.PaymentGateway.Queries.GetPaymentGatewayById;

public record GetPaymentGatewayByIdQuery(int Id) : IRequest<Result<PaymentGatewayDto>>;
