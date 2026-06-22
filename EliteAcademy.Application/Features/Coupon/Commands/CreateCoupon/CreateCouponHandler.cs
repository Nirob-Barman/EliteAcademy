using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Mappers;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Coupon.Commands.CreateCoupon;

public class CreateCouponHandler : IRequestHandler<CreateCouponCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;
    private readonly IAuditLogService _auditLogService;

    public CreateCouponHandler(
        IApplicationDbContext context,
        IUserContextService userContextService,
        IAuditLogService auditLogService)
    {
        _context = context;
        _userContextService = userContextService;
        _auditLogService = auditLogService;
    }

    public async Task<Result<bool>> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var code = dto.Code.Trim().ToUpper();

        if (await _context.Coupons.AnyAsync(c => c.Code == code, cancellationToken))
            return Result<bool>.FailField("Code", "This coupon code already exists.");

        var entity = CouponMapper.ToEntity(dto);
        entity.CreatedBy = _userContextService.UserId;
        _context.Coupons.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync("Coupon", "Create",
            details: $"Created coupon \"{code}\" ({dto.DiscountPercent}% off)");

        return Result<bool>.Ok(true, "Coupon created.");
    }
}
