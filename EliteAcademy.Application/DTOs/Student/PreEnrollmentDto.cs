using EliteAcademy.Domain.Enums;

namespace EliteAcademy.Application.DTOs.Student
{
    public class PreEnrollmentDto
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public string? ClassName { get; set; }
        public string? ClassImage { get; set; }
        public string? InstructorName { get; set; }
        public decimal Price { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string? CouponCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalPrice => Price - DiscountAmount;
    }
}
