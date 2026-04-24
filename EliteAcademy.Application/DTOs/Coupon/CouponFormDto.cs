namespace EliteAcademy.Application.DTOs.Coupon
{
    public class CouponFormDto
    {
        public string Code { get; set; } = string.Empty;
        public decimal DiscountPercent { get; set; }
        public int MaxUsages { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
