using EliteAcademy.Domain.Common;
using EliteAcademy.Domain.Entities.Student;
using EliteAcademy.Domain.Enums;

namespace EliteAcademy.Domain.Entities
{
    public class PaymentTransaction : BaseEntity
    {
        public int PreEnrollmentId { get; private set; }
        public PreEnrollment? PreEnrollment { get; private set; }

        public int GatewayId { get; private set; }
        public PaymentGateway? Gateway { get; private set; }

        public decimal Amount { get; private set; }
        public string? SessionRef { get; private set; }
        public PaymentTransactionStatus Status { get; private set; } = PaymentTransactionStatus.Pending;

        private PaymentTransaction() { }

        public static DomainResult<PaymentTransaction> Create(
            int preEnrollmentId,
            int gatewayId,
            decimal amount,
            string createdBy)
        {
            if (preEnrollmentId <= 0)
                return DomainResult<PaymentTransaction>.Fail("Invalid pre-enrollment.");

            if (gatewayId <= 0)
                return DomainResult<PaymentTransaction>.Fail("Invalid payment gateway.");

            if (amount <= 0)
                return DomainResult<PaymentTransaction>.Fail("Payment amount must be greater than zero.");

            if (string.IsNullOrWhiteSpace(createdBy))
                return DomainResult<PaymentTransaction>.Fail("Creator is required.");

            var transaction = new PaymentTransaction
            {
                PreEnrollmentId = preEnrollmentId,
                GatewayId = gatewayId,
                Amount = amount,
                Status = PaymentTransactionStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            return DomainResult<PaymentTransaction>.Ok(transaction);
        }

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
