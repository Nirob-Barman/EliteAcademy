namespace EliteAcademy.Application.DTOs.Coupon
{
    public class CouponDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal DiscountPercent { get; set; }
        public int MaxUsages { get; set; }
        public int UsageCount { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
        public bool IsFull => MaxUsages > 0 && UsageCount >= MaxUsages;
    }
}
