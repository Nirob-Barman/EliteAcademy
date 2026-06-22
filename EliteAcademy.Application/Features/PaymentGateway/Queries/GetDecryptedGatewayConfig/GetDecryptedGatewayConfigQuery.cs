using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.PaymentGateway.Queries.GetDecryptedGatewayConfig;

public record GetDecryptedGatewayConfigQuery(int Id) : IRequest<Result<string>>;
