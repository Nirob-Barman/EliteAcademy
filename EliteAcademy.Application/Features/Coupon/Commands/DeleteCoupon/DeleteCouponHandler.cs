using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Coupon.Commands.DeleteCoupon;

public class DeleteCouponHandler : IRequestHandler<DeleteCouponCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public DeleteCouponHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<bool>> Handle(DeleteCouponCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (entity == null)
            return Result<bool>.Fail("Coupon not found.");

        entity.MarkDeleted();
        _context.Coupons.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Ok(true, "Coupon deleted.");
    }
}
