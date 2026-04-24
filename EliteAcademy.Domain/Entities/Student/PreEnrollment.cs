using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Enums;

namespace EliteAcademy.Domain.Entities.Student
{
    public class PreEnrollment : BaseEntity
    {
        public int ClassId { get; set; }
        public Class? Class { get; set; }
        public string? StudentId { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public string? CouponCode { get; set; }
        public decimal DiscountAmount { get; set; }
    }
}
