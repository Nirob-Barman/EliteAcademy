using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Payment;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.PaymentGateway.Queries.GetPaymentGatewayById;

public class GetPaymentGatewayByIdHandler : IRequestHandler<GetPaymentGatewayByIdQuery, Result<PaymentGatewayDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPaymentGatewayByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaymentGatewayDto>> Handle(GetPaymentGatewayByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.PaymentGateways
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);

        if (entity == null)
            return Result<PaymentGatewayDto>.Fail("Gateway not found.");

        return Result<PaymentGatewayDto>.Ok(new PaymentGatewayDto
        {
            Id = entity.Id,
            Slug = entity.Slug,
            Name = entity.Name,
            IsActive = entity.IsActive,
            IsSandbox = entity.IsSandbox,
            CreatedAt = entity.CreatedAt
        });
    }
}
