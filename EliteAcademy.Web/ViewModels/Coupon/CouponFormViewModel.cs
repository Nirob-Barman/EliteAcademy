using System.ComponentModel.DataAnnotations;

namespace EliteAcademy.Web.ViewModels.Coupon
{
    public class CouponFormViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        [Display(Name = "Coupon Code")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [Range(1, 100, ErrorMessage = "Discount must be between 1 and 100%.")]
        [Display(Name = "Discount (%)")]
        public decimal DiscountPercent { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Max usages must be 0 (unlimited) or greater.")]
        [Display(Name = "Max Usages (0 = unlimited)")]
        public int MaxUsages { get; set; }

        [Required]
        [Display(Name = "Expires At")]
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMonths(1);

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
