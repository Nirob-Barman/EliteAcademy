using EliteAcademy.Application.DTOs.Payment;
using System.ComponentModel.DataAnnotations;

namespace EliteAcademy.Web.ViewModels.Student
{
    public class CheckoutViewModel
    {
        public int PreEnrollmentId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount => Price - DiscountAmount;
        public string? CouponCode { get; set; }

        [Required(ErrorMessage = "Please select a payment gateway.")]
        public string? SelectedGatewaySlug { get; set; }

        public List<PaymentGatewayDto> Gateways { get; set; } = new();
    }
}
