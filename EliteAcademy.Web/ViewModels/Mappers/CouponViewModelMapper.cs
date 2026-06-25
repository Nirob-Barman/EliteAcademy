using EliteAcademy.Application.DTOs.Coupon;
using EliteAcademy.Web.ViewModels.Coupon;

namespace EliteAcademy.Web.ViewModels.Mappers
{
    public static class CouponViewModelMapper
    {
        public static CouponFormDto ToDto(CouponFormViewModel vm) => new()
        {
            Code = vm.Code,
            DiscountPercent = vm.DiscountPercent,
            MaxUsages = vm.MaxUsages,
            ExpiresAt = vm.ExpiresAt,
            IsActive = vm.IsActive
        };

        public static CouponFormViewModel ToVm(CouponDto dto) => new()
        {
            Id = dto.Id,
            Code = dto.Code,
            DiscountPercent = dto.DiscountPercent,
            MaxUsages = dto.MaxUsages,
            ExpiresAt = dto.ExpiresAt,
            IsActive = dto.IsActive
        };
    }
}
