using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Coupon;
using EliteAcademy.Application.Mappers;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Coupon.Queries.GetAllCoupons;

public class GetAllCouponsHandler : IRequestHandler<GetAllCouponsQuery, Result<List<CouponDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllCouponsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<CouponDto>>> Handle(GetAllCouponsQuery request, CancellationToken cancellationToken)
    {
        var all = (await _context.Coupons.AsNoTracking().ToListAsync(cancellationToken))
            .Select(CouponMapper.ToDto)
            .ToList();
        return Result<List<CouponDto>>.Ok(all);
    }
}
