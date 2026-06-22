using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Coupon.Queries.ValidateCoupon;

public class ValidateCouponHandler : IRequestHandler<ValidateCouponQuery, Result<decimal>>
{
    private readonly IApplicationDbContext _context;

    public ValidateCouponHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<decimal>> Handle(ValidateCouponQuery request, CancellationToken cancellationToken)
    {
        var upper = request.Code.Trim().ToUpper();
        var coupon = await _context.Coupons
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == upper, cancellationToken);

        if (coupon == null)
            return Result<decimal>.Fail("Invalid coupon code.");
        if (!coupon.IsActive)
            return Result<decimal>.Fail("This coupon is not active.");
        if (DateTime.UtcNow > coupon.ExpiresAt)
            return Result<decimal>.Fail("This coupon has expired.");
        if (coupon.MaxUsages > 0 && coupon.UsageCount >= coupon.MaxUsages)
            return Result<decimal>.Fail("This coupon has reached its usage limit.");

        var discount = Math.Round(request.Price * coupon.DiscountPercent / 100, 2);
        return Result<decimal>.Ok(discount, $"{coupon.DiscountPercent}% discount applied.");
    }
}
