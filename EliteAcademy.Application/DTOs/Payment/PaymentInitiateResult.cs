namespace EliteAcademy.Application.DTOs.Payment
{
    public class PaymentInitiateResult
    {
        public bool Success { get; set; }
        public string? RedirectUrl { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
