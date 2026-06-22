using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Enums;
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
        if (pe.PaymentStatus != PaymentStatus.Pending)
            return Result<bool>.Fail("Cannot apply coupon to a paid selection.");

        var cls = await _context.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == pe.ClassId, cancellationToken);
        if (cls == null)
            return Result<bool>.Fail("Class not found.");

        var upper = request.CouponCode.Trim().ToUpper();
        var coupon = await _context.Coupons.AsNoTracking().FirstOrDefaultAsync(c => c.Code == upper, cancellationToken);

        if (coupon == null)
            return Result<bool>.Fail("Invalid coupon code.");
        if (!coupon.IsActive)
            return Result<bool>.Fail("This coupon is not active.");
        if (DateTime.UtcNow > coupon.ExpiresAt)
            return Result<bool>.Fail("This coupon has expired.");
        if (coupon.MaxUsages > 0 && coupon.UsageCount >= coupon.MaxUsages)
            return Result<bool>.Fail("This coupon has reached its usage limit.");

        pe.CouponCode = upper;
        pe.DiscountAmount = Math.Round(cls.Price * coupon.DiscountPercent / 100, 2);
        pe.UpdatedAt = DateTime.UtcNow;
        pe.UpdatedBy = studentId;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true, $"{coupon.DiscountPercent}% discount applied! You save ${pe.DiscountAmount:0.00}.");
    }
}
