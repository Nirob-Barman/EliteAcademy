using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Coupon;
using EliteAcademy.Application.Mappers;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Coupon.Queries.GetCouponById;

public class GetCouponByIdHandler : IRequestHandler<GetCouponByIdQuery, Result<CouponDto?>>
{
    private readonly IApplicationDbContext _context;

    public GetCouponByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CouponDto?>> Handle(GetCouponByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Coupons
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        return entity == null
            ? Result<CouponDto?>.Fail("Coupon not found.")
            : Result<CouponDto?>.Ok(CouponMapper.ToDto(entity));
    }
}
