using EliteAcademy.Domain.Events;

namespace EliteAcademy.Domain.Entities
{
    public class PaymentGateway : BaseEntity
    {
        public string Slug { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Config { get; set; } = string.Empty;   // encrypted JSON
        public bool IsActive { get; set; } = true;
        public bool IsSandbox { get; set; } = true;

        public ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();

        public void MarkDeleted() => AddDomainEvent(new PaymentGatewayDeletedEvent(Name, Id));
    }
}
