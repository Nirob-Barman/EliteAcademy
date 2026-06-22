using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Coupon.Commands.ToggleCoupon;

public class ToggleCouponHandler : IRequestHandler<ToggleCouponCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;
    private readonly IAuditLogService _auditLogService;

    public ToggleCouponHandler(
        IApplicationDbContext context,
        IUserContextService userContextService,
        IAuditLogService auditLogService)
    {
        _context = context;
        _userContextService = userContextService;
        _auditLogService = auditLogService;
    }

    public async Task<Result<bool>> Handle(ToggleCouponCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (entity == null)
            return Result<bool>.Fail("Coupon not found.");

        entity.IsActive = !entity.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = _userContextService.UserId;

        await _context.SaveChangesAsync(cancellationToken);

        var action = entity.IsActive ? "Activate" : "Deactivate";
        await _auditLogService.LogAsync("Coupon", action,
            details: $"Coupon \"{entity.Code}\" {(entity.IsActive ? "activated" : "deactivated")}");

        return Result<bool>.Ok(true, entity.IsActive ? "Coupon activated." : "Coupon deactivated.");
    }
}
