using EliteAcademy.Domain.Common;
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

        public static DomainResult<PreEnrollment> Create(string studentId, Class? cls)
        {
            if (cls == null || cls.Status != ClassStatus.Approved)
                return DomainResult<PreEnrollment>.Fail("Class is not available.");
            if (cls.AvailableSeats <= 0)
                return DomainResult<PreEnrollment>.Fail("No available seats.");

            return DomainResult<PreEnrollment>.Ok(new PreEnrollment
            {
                ClassId = cls.Id,
                StudentId = studentId,
                PaymentStatus = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = studentId
            });
        }
    }
}
