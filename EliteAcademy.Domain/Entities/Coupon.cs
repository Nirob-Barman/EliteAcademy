using EliteAcademy.Domain.Common;
using EliteAcademy.Domain.Events;

namespace EliteAcademy.Domain.Entities
{
    public class Coupon : BaseEntity
    {
        public string Code { get; private set; } = string.Empty;
        public decimal DiscountPercent { get; private set; }   // 1–100
        public int MaxUsages { get; private set; }
        public int UsageCount { get; set; }
        public DateTime ExpiresAt { get; private set; }
        public bool IsActive { get; set; } = true;

        public static DomainResult<Coupon> Create(string code, decimal discountPercent, int maxUsages, DateTime expiresAt, bool isActive)
        {
            var validationError = Validate(code, discountPercent, expiresAt);
            if (validationError != null)
                return DomainResult<Coupon>.Fail(validationError);

            return DomainResult<Coupon>.Ok(new Coupon
            {
                Code = code.Trim().ToUpper(),
                DiscountPercent = discountPercent,
                MaxUsages = maxUsages,
                ExpiresAt = expiresAt,
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow
            });
        }

        public DomainResult<bool> UpdateDetails(string code, decimal discountPercent, int maxUsages, DateTime expiresAt, bool isActive)
        {
            var validationError = Validate(code, discountPercent, expiresAt);
            if (validationError != null)
                return DomainResult<bool>.Fail(validationError);

            Code = code.Trim().ToUpper();
            DiscountPercent = discountPercent;
            MaxUsages = maxUsages;
            ExpiresAt = expiresAt;
            IsActive = isActive;
            UpdatedAt = DateTime.UtcNow;

            return DomainResult<bool>.Ok(true);
        }

        public void MarkDeleted() => AddDomainEvent(new CouponDeletedEvent(Code, Id));

        private static string? Validate(string code, decimal discountPercent, DateTime expiresAt)
        {
            if (string.IsNullOrWhiteSpace(code))
                return "Coupon code is required.";
            if (discountPercent < 1 || discountPercent > 100)
                return "Discount percent must be between 1 and 100.";
            if (expiresAt <= DateTime.UtcNow)
                return "Expiry date must be in the future.";

            return null;
        }

        public DomainResult<bool> EnsureUsable()
        {
            if (!IsActive)
                return DomainResult<bool>.Fail("This coupon is not active.");
            if (DateTime.UtcNow > ExpiresAt)
                return DomainResult<bool>.Fail("This coupon has expired.");
            if (MaxUsages > 0 && UsageCount >= MaxUsages)
                return DomainResult<bool>.Fail("This coupon has reached its usage limit.");

            return DomainResult<bool>.Ok(true);
        }

        public decimal CalculateDiscount(decimal price) => Math.Round(price * DiscountPercent / 100, 2);

        public void RecordUsage()
        {
            UsageCount++;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
