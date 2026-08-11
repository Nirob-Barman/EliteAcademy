using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Student.Commands.ApplyCoupon;

public class ApplyCouponHandler : IRequestHandler<ApplyCouponCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;

    public ApplyCouponHandler(IApplicationDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<Result<bool>> Handle(ApplyCouponCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CouponCode))
            return Result<bool>.Fail("Please enter a coupon code.");

        var studentId = _userContextService.UserId!;
        var pe = await _context.PreEnrollments.FirstOrDefaultAsync(p => p.Id == request.PreEnrollmentId, cancellationToken);
        if (pe == null || pe.StudentId != studentId)
            return Result<bool>.Fail("Selection not found.");

        var cls = await _context.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == pe.ClassId, cancellationToken);
        if (cls == null)
            return Result<bool>.Fail("Class not found.");

        var upper = request.CouponCode.Trim().ToUpper();
        var coupon = await _context.Coupons.AsNoTracking().FirstOrDefaultAsync(c => c.Code == upper, cancellationToken);

        if (coupon == null)
            return Result<bool>.Fail("Invalid coupon code.");

        var usableResult = coupon.EnsureUsable();
        if (!usableResult.IsSuccess)
            return Result<bool>.Fail(usableResult.Error);

        var discountAmount = coupon.CalculateDiscount(cls.Price);
        var applyResult = pe.ApplyCoupon(upper, discountAmount);
        if (!applyResult.IsSuccess)
            return Result<bool>.Fail(applyResult.Error);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true, $"{coupon.DiscountPercent}% discount applied! You save ${pe.DiscountAmount:0.00}.");
    }
}
