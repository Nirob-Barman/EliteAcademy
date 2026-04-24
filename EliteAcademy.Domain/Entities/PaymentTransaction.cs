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
        public PaymentTransactionStatus Status { get; set; } = PaymentTransactionStatus.Pending;
    }
}
