using EliteAcademy.Application.DTOs.Payment;
using EliteAcademy.Application.Wrappers;

namespace EliteAcademy.Application.Interfaces.Services
{
    public interface IPaymentGatewayService
    {
        Task<Result<List<PaymentGatewayDto>>> GetAllAsync();
        Task<Result<PaymentGatewayDto>> GetByIdAsync(int id);
        Task<Result<string>> GetDecryptedConfigAsync(int id);
        Task<Result<bool>> CreateAsync(PaymentGatewayFormDto dto);
        Task<Result<bool>> UpdateAsync(int id, PaymentGatewayFormDto dto);
        Task<Result<bool>> ToggleActiveAsync(int id);
        Task<Result<bool>> DeleteAsync(int id);
    }
}
