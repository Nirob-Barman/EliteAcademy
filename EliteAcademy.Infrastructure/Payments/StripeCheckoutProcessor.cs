using EliteAcademy.Application.DTOs.Payment;
using EliteAcademy.Application.Interfaces;
using Stripe;
using Stripe.Checkout;

namespace EliteAcademy.Infrastructure.Payments
{
    public class StripeCheckoutProcessor : IPaymentProcessor
    {
        public string Slug => "stripe_checkout";

        public async Task<PaymentInitiateResult> InitiateAsync(
            Dictionary<string, string> config,
            decimal amount,
            int txId,
            string successUrl,
            string cancelUrl)
        {
            if (!config.TryGetValue("secret_key", out var secretKey) || string.IsNullOrWhiteSpace(secretKey))
                return new PaymentInitiateResult { Success = false, ErrorMessage = "Stripe secret key not configured." };

            StripeConfiguration.ApiKey = secretKey;

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            UnitAmount = (long)(amount * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Class Enrollment"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = successUrl + "&session_id={CHECKOUT_SESSION_ID}",
                CancelUrl  = cancelUrl
            };

            try
            {
                var service = new SessionService();
                var session = await service.CreateAsync(options);

                return new PaymentInitiateResult
                {
                    Success     = true,
                    RedirectUrl = session.Url
                };
            }
            catch (StripeException ex)
            {
                return new PaymentInitiateResult
                {
                    Success      = false,
                    ErrorMessage = ex.StripeError?.Message ?? ex.Message
                };
            }
        }

        public async Task<bool> VerifyAsync(
            Dictionary<string, string> config,
            Dictionary<string, string> callbackParams)
        {
            if (!config.TryGetValue("secret_key", out var secretKey) || string.IsNullOrWhiteSpace(secretKey))
                return false;

            if (!callbackParams.TryGetValue("session_id", out var sessionId) || string.IsNullOrWhiteSpace(sessionId))
                return false;

            StripeConfiguration.ApiKey = secretKey;

            try
            {
                var service = new SessionService();
                var session = await service.GetAsync(sessionId);
                return session?.PaymentStatus == "paid";
            }
            catch (StripeException)
            {
                return false;
            }
        }
    }
}
