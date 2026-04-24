using EliteAcademy.Application.DTOs.Account;
using EliteAcademy.Application.DTOs.Identity;
using EliteAcademy.Application.Wrappers;

namespace EliteAcademy.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<Result<List<LoginHistoryItemDto>>> GetMyLoginHistoryAsync();
        Task<Result<string>> RegisterAsync(RegisterDto model, Stream? imageStream, string? imageFileName);
        Task<Result<string>> LoginAsync(LoginDto model);
        Task<Result<string>> LogoutAsync();
        Task<bool> CheckPasswordAsync(ApplicationUserDto userDto, string password);
        Task<bool> EmailExistsAsync(string email);
        Task<Result<EditProfileDto>> GetMyProfileAsync();
        Task<Result<bool>> UpdateMyProfileAsync(EditProfileDto dto, Stream? imageStream, string? imageFileName);
        Task<Result<bool>> ChangePasswordAsync(string currentPassword, string newPassword);
        Task<Result<EditProfileDto>> GetProfileAsync(string userId);
        Task<Result<bool>> UpdateProfileAsync(string userId, EditProfileDto model);
        
        Task<Result<bool>> ForgotPasswordAsync(string email, string callbackUrl);
        Task<Result<string>> GeneratePasswordResetTokenAsync(string email);
        Task<Result<bool>> ResetPasswordAsync(string email, string token, string newPassword);
        Task<Result<List<string>>> GetAllRolesNonAdminAsync();
        Task<Result<bool>> AssignRoleToUserAsync(string userId, string roleName);
        Task<Result<bool>> CreateRoleAsync(string roleName);
    }
}
