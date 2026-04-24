using EliteAcademy.Application.Wrappers;

namespace EliteAcademy.Application.Interfaces.Services
{
    public interface IPaymentService
    {
        Task<Result<string>> InitiateAsync(int preEnrollmentId, string gatewaySlug, string baseUrl);
        Task<Result<bool>> HandleSuccessAsync(int txId, string gatewaySlug, Dictionary<string, string> callbackParams);
        Task<Result<bool>> HandleCancelAsync(int txId);
    }
}
