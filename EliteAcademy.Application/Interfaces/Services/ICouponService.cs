using EliteAcademy.Application.DTOs.Coupon;
using EliteAcademy.Application.Wrappers;

namespace EliteAcademy.Application.Interfaces.Services
{
    public interface ICouponService
    {
        Task<Result<List<CouponDto>>> GetAllAsync();
        Task<Result<CouponDto?>>      GetByIdAsync(int id);
        Task<Result<bool>>            CreateAsync(CouponFormDto dto);
        Task<Result<bool>>            UpdateAsync(int id, CouponFormDto dto);
        Task<Result<bool>>            DeleteAsync(int id);
        Task<Result<bool>>            ToggleActiveAsync(int id);
        Task<Result<decimal>>         ValidateAndGetDiscountAsync(string code, decimal price);
    }
}
