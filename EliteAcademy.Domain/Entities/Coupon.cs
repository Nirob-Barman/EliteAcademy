namespace EliteAcademy.Domain.Entities
{
    public class Coupon : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public decimal DiscountPercent { get; set; }   // 1–100
        public int MaxUsages { get; set; }
        public int UsageCount { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
