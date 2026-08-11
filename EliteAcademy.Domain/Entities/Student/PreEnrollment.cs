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
        public PaymentStatus PaymentStatus { get; private set; } = PaymentStatus.Pending;
        public string? CouponCode { get; private set; }
        public decimal DiscountAmount { get; private set; }

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
                CreatedAt = DateTime.UtcNow,
                CreatedBy = studentId
            });
        }

        public DomainResult<bool> MarkPaid()
        {
            if (PaymentStatus != PaymentStatus.Pending)
                return DomainResult<bool>.Fail("Already paid.");

            PaymentStatus = PaymentStatus.Paid;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = StudentId;

            return DomainResult<bool>.Ok(true);
        }

        public DomainResult<bool> ApplyCoupon(string code, decimal discountAmount)
        {
            if (PaymentStatus != PaymentStatus.Pending)
                return DomainResult<bool>.Fail("Cannot apply coupon to a paid selection.");

            CouponCode = code;
            DiscountAmount = discountAmount;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = StudentId;

            return DomainResult<bool>.Ok(true);
        }

        public DomainResult<bool> RemoveCoupon()
        {
            if (PaymentStatus != PaymentStatus.Pending)
                return DomainResult<bool>.Fail("Cannot modify a paid selection.");

            CouponCode = null;
            DiscountAmount = 0;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = StudentId;

            return DomainResult<bool>.Ok(true);
        }
    }
}
