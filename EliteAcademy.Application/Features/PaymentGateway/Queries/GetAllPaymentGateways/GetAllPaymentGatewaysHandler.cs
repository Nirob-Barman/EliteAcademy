using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Payment;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.PaymentGateway.Queries.GetAllPaymentGateways;

public class GetAllPaymentGatewaysHandler : IRequestHandler<GetAllPaymentGatewaysQuery, Result<List<PaymentGatewayDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllPaymentGatewaysHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<PaymentGatewayDto>>> Handle(GetAllPaymentGatewaysQuery request, CancellationToken cancellationToken)
    {
        var all = await _context.PaymentGateways.AsNoTracking().ToListAsync(cancellationToken);
        var txCounts = (await _context.PaymentTransactions.AsNoTracking().ToListAsync(cancellationToken))
            .GroupBy(t => t.GatewayId)
            .ToDictionary(g => g.Key, g => g.Count());

        var dtos = all.Select(g => new PaymentGatewayDto
        {
            Id = g.Id,
            Slug = g.Slug,
            Name = g.Name,
            IsActive = g.IsActive,
            IsSandbox = g.IsSandbox,
            CreatedAt = g.CreatedAt,
            TransactionCount = txCounts.GetValueOrDefault(g.Id)
        }).ToList();

        return Result<List<PaymentGatewayDto>>.Ok(dtos);
    }
}
