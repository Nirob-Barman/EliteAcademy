using EliteAcademy.Application.DTOs.Payment;
using EliteAcademy.Application.Interfaces;

namespace EliteAcademy.Infrastructure.Payments
{
    public class MockPaymentProcessor : IPaymentProcessor
    {
        public string Slug => "mock";

        public Task<PaymentInitiateResult> InitiateAsync(
            Dictionary<string, string> config,
            decimal amount,
            int txId,
            string successUrl,
            string cancelUrl)
        {
            // Demo: redirect immediately to success
            return Task.FromResult(new PaymentInitiateResult
            {
                Success = true,
                RedirectUrl = successUrl
            });
        }

        public Task<bool> VerifyAsync(
            Dictionary<string, string> config,
            Dictionary<string, string> callbackParams)
        {
            return Task.FromResult(true);
        }
    }
}
