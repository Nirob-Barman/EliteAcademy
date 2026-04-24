using EliteAcademy.Application.DTOs.Coupon;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Persistence;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Mappers;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities;

namespace EliteAcademy.Application.Services
{
    public class CouponService : ICouponService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContextService _userContextService;
        private readonly IAuditLogService _auditLogService;

        public CouponService(
            IUnitOfWork unitOfWork,
            IUserContextService userContextService,
            IAuditLogService auditLogService)
        {
            _unitOfWork         = unitOfWork;
            _userContextService = userContextService;
            _auditLogService    = auditLogService;
        }

        public async Task<Result<List<CouponDto>>> GetAllAsync()
        {
            var all = (await _unitOfWork.Repository<Coupon>().GetAllAsync())
                .Select(CouponMapper.ToDto)
                .ToList();
            return Result<List<CouponDto>>.Ok(all);
        }

        public async Task<Result<CouponDto?>> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.Repository<Coupon>().GetByIdAsync(id);
            return entity == null
                ? Result<CouponDto?>.Fail("Coupon not found.")
                : Result<CouponDto?>.Ok(CouponMapper.ToDto(entity));
        }

        public async Task<Result<bool>> CreateAsync(CouponFormDto dto)
        {
            var code = dto.Code.Trim().ToUpper();
            var exists = await _unitOfWork.Repository<Coupon>()
                .AnyAsync(c => c.Code == code);
            if (exists)
                return Result<bool>.FailField("Code", "This coupon code already exists.");

            var entity = CouponMapper.ToEntity(dto);
            entity.CreatedBy = _userContextService.UserId;
            await _unitOfWork.Repository<Coupon>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync("Coupon", "Create",
                details: $"Created coupon \"{code}\" ({dto.DiscountPercent}% off)");

            return Result<bool>.Ok(true, "Coupon created.");
        }

        public async Task<Result<bool>> UpdateAsync(int id, CouponFormDto dto)
        {
            var entity = await _unitOfWork.Repository<Coupon>().GetByIdAsync(id);
            if (entity == null)
                return Result<bool>.Fail("Coupon not found.");

            var code = dto.Code.Trim().ToUpper();
            var duplicate = await _unitOfWork.Repository<Coupon>()
                .AnyAsync(c => c.Code == code && c.Id != id);
            if (duplicate)
                return Result<bool>.FailField("Code", "This coupon code is already used.");

            var oldCode = entity.Code;
            entity.Code            = code;
            entity.DiscountPercent = dto.DiscountPercent;
            entity.MaxUsages       = dto.MaxUsages;
            entity.ExpiresAt       = dto.ExpiresAt;
            entity.IsActive        = dto.IsActive;
            entity.UpdatedAt       = DateTime.UtcNow;
            entity.UpdatedBy       = _userContextService.UserId;

            _unitOfWork.Repository<Coupon>().Update(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync("Coupon", "Update",
                details: $"Updated coupon \"{oldCode}\" (ID: {id})");

            return Result<bool>.Ok(true, "Coupon updated.");
        }

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.Repository<Coupon>().GetByIdAsync(id);
            if (entity == null)
                return Result<bool>.Fail("Coupon not found.");

            var code = entity.Code;
            _unitOfWork.Repository<Coupon>().Remove(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync("Coupon", "Delete",
                details: $"Deleted coupon \"{code}\" (ID: {id})");

            return Result<bool>.Ok(true, "Coupon deleted.");
        }

        public async Task<Result<bool>> ToggleActiveAsync(int id)
        {
            var entity = await _unitOfWork.Repository<Coupon>().GetByIdAsync(id);
            if (entity == null)
                return Result<bool>.Fail("Coupon not found.");

            entity.IsActive  = !entity.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _userContextService.UserId;

            _unitOfWork.Repository<Coupon>().Update(entity);
            await _unitOfWork.SaveChangesAsync();

            var action = entity.IsActive ? "Activate" : "Deactivate";
            await _auditLogService.LogAsync("Coupon", action,
                details: $"Coupon \"{entity.Code}\" {(entity.IsActive ? "activated" : "deactivated")}");

            return Result<bool>.Ok(true, entity.IsActive ? "Coupon activated." : "Coupon deactivated.");
        }

        public async Task<Result<decimal>> ValidateAndGetDiscountAsync(string code, decimal price)
        {
            var upper = code.Trim().ToUpper();
            var coupon = await _unitOfWork.Repository<Coupon>()
                .FirstOrDefaultAsync(c => c.Code == upper);

            if (coupon == null)
                return Result<decimal>.Fail("Invalid coupon code.");
            if (!coupon.IsActive)
                return Result<decimal>.Fail("This coupon is not active.");
            if (DateTime.UtcNow > coupon.ExpiresAt)
                return Result<decimal>.Fail("This coupon has expired.");
            if (coupon.MaxUsages > 0 && coupon.UsageCount >= coupon.MaxUsages)
                return Result<decimal>.Fail("This coupon has reached its usage limit.");

            var discount = Math.Round(price * coupon.DiscountPercent / 100, 2);
            return Result<decimal>.Ok(discount, $"{coupon.DiscountPercent}% discount applied.");
        }
    }
}
