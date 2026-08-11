using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Coupon.Commands.UpdateCoupon;

public class UpdateCouponHandler : IRequestHandler<UpdateCouponCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;
    private readonly IAuditLogService _auditLogService;

    public UpdateCouponHandler(
        IApplicationDbContext context,
        IUserContextService userContextService,
        IAuditLogService auditLogService)
    {
        _context = context;
        _userContextService = userContextService;
        _auditLogService = auditLogService;
    }

    public async Task<Result<bool>> Handle(UpdateCouponCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var entity = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (entity == null)
            return Result<bool>.Fail("Coupon not found.");

        var code = dto.Code.Trim().ToUpper();
        if (await _context.Coupons.AnyAsync(c => c.Code == code && c.Id != request.Id, cancellationToken))
            return Result<bool>.FailField("Code", "This coupon code is already used.");

        var oldCode = entity.Code;
        var updateResult = entity.UpdateDetails(code, dto.DiscountPercent, dto.MaxUsages, dto.ExpiresAt, dto.IsActive);
        if (!updateResult.IsSuccess)
            return Result<bool>.Fail(updateResult.Error);

        entity.UpdatedBy = _userContextService.UserId;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync("Coupon", "Update",
            details: $"Updated coupon \"{oldCode}\" (ID: {request.Id})");

        return Result<bool>.Ok(true, "Coupon updated.");
    }
}
