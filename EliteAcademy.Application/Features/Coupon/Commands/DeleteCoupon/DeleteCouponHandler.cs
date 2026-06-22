using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Coupon.Commands.DeleteCoupon;

public class DeleteCouponHandler : IRequestHandler<DeleteCouponCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public DeleteCouponHandler(IApplicationDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<Result<bool>> Handle(DeleteCouponCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Coupons
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (entity == null)
            return Result<bool>.Fail("Coupon not found.");

        var code = entity.Code;
        _context.Coupons.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync("Coupon", "Delete",
            details: $"Deleted coupon \"{code}\" (ID: {request.Id})");

        return Result<bool>.Ok(true, "Coupon deleted.");
    }
}
