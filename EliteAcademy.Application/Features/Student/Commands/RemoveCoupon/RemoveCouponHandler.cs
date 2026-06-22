using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Student.Commands.RemoveCoupon;

public class RemoveCouponHandler : IRequestHandler<RemoveCouponCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;

    public RemoveCouponHandler(IApplicationDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<Result<bool>> Handle(RemoveCouponCommand request, CancellationToken cancellationToken)
    {
        var studentId = _userContextService.UserId!;
        var pe = await _context.PreEnrollments.FirstOrDefaultAsync(p => p.Id == request.PreEnrollmentId, cancellationToken);
        if (pe == null || pe.StudentId != studentId)
            return Result<bool>.Fail("Selection not found.");
        if (pe.PaymentStatus != PaymentStatus.Pending)
            return Result<bool>.Fail("Cannot modify a paid selection.");

        pe.CouponCode = null;
        pe.DiscountAmount = 0;
        pe.UpdatedAt = DateTime.UtcNow;
        pe.UpdatedBy = studentId;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true, "Coupon removed.");
    }
}
