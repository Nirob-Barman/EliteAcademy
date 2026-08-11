using EliteAcademy.Application.DTOs.Coupon;
using EliteAcademy.Domain.Entities;

namespace EliteAcademy.Application.Mappers
{
    public static class CouponMapper
    {
        public static CouponDto ToDto(Coupon entity) => new()
        {
            Id = entity.Id,
            Code = entity.Code,
            DiscountPercent = entity.DiscountPercent,
            MaxUsages = entity.MaxUsages,
            UsageCount = entity.UsageCount,
            ExpiresAt = entity.ExpiresAt,
            IsActive = entity.IsActive
        };

    }
}
