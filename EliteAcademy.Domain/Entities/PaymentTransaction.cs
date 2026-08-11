using EliteAcademy.Domain.Common;
using EliteAcademy.Domain.Entities.Student;
using EliteAcademy.Domain.Enums;

namespace EliteAcademy.Domain.Entities
{
    public class PaymentTransaction : BaseEntity
    {
        public int PreEnrollmentId { get; set; }
        public PreEnrollment? PreEnrollment { get; set; }

        public int GatewayId { get; set; }
        public PaymentGateway? Gateway { get; set; }

        public decimal Amount { get; set; }
        public string? SessionRef { get; set; }
        public PaymentTransactionStatus Status { get; private set; } = PaymentTransactionStatus.Pending;

        public DomainResult<bool> MarkSuccess()
        {
            if (Status != PaymentTransactionStatus.Pending)
                return DomainResult<bool>.Fail("Transaction already processed.");

            Status = PaymentTransactionStatus.Success;
            UpdatedAt = DateTime.UtcNow;

            return DomainResult<bool>.Ok(true);
        }

        public void MarkFailed()
        {
            Status = PaymentTransactionStatus.Failed;
            UpdatedAt = DateTime.UtcNow;
        }

        public DomainResult<bool> Cancel()
        {
            if (Status != PaymentTransactionStatus.Pending)
                return DomainResult<bool>.Fail("Only pending transactions can be cancelled.");

            Status = PaymentTransactionStatus.Cancelled;
            UpdatedAt = DateTime.UtcNow;

            return DomainResult<bool>.Ok(true);
        }
    }
}
