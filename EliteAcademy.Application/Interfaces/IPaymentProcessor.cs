using EliteAcademy.Application.DTOs.Payment;

namespace EliteAcademy.Application.Interfaces
{
    public interface IPaymentProcessor
    {
        string Slug { get; }

        Task<PaymentInitiateResult> InitiateAsync(
            Dictionary<string, string> config,
            decimal amount,
            int txId,
            string successUrl,
            string cancelUrl);

        Task<bool> VerifyAsync(
            Dictionary<string, string> config,
            Dictionary<string, string> callbackParams);
    }
}
