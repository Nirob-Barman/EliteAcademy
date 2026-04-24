using EliteAcademy.Application.Interfaces;

namespace EliteAcademy.Infrastructure.Payments
{
    public class PaymentProcessorFactory : IPaymentProcessorFactory
    {
        private readonly IEnumerable<IPaymentProcessor> _processors;

        public PaymentProcessorFactory(IEnumerable<IPaymentProcessor> processors)
        {
            _processors = processors;
        }

        public IPaymentProcessor? GetProcessor(string slug) =>
            _processors.FirstOrDefault(p =>
                string.Equals(p.Slug, slug, StringComparison.OrdinalIgnoreCase));
    }
}
