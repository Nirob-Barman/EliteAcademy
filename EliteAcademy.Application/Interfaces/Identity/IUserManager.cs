using EliteAcademy.Domain.Entities.Account;

namespace EliteAcademy.Application.Interfaces.Identity
{
    public interface IUserManager
    {
        Task<(bool Succeeded, string? UserId, List<string> Errors)> CreateAsync(AppUser user, string password);
        Task<(bool Succeeded, List<string> Errors)> UpdateAsync(AppUser user);
        Task<(bool Succeeded, List<string> Errors)> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<string> GeneratePasswordResetTokenAsync(string email);
        Task<AppUser?> FindByEmailAsync(string email);
        Task<AppUser?> FindByIdAsync(string id);
        Task<string[]> GetRolesAsync(AppUser user);
        Task<bool> CheckPasswordAsync(AppUser user, string password);
        Task<string> GeneratePasswordResetTokenAsync(AppUser user);
        Task<(bool Succeeded, List<string> Errors)> ResetPasswordAsync(AppUser user, string token, string newPassword);
        //Task<bool> IsInRoleAsync(AppUser user, string role);
        Task<(bool Succeeded, List<string> Errors)> AddToRoleAsync(AppUser user, string roleName);
        Task<(bool Succeeded, List<string> Errors)> RemoveFromRoleAsync(AppUser user, string roleName);
        Task<IEnumerable<AppUser>> GetAllUsersAsync();
        Task<IEnumerable<AppUser>> GetUsersByRoleAsync(string role);
        Task<bool> IsUserInRoleAsync(AppUser user, string role);
        Task<(bool Succeeded, List<string> Errors)> BanUserAsync(string userId);
        Task<(bool Succeeded, List<string> Errors)> UnbanUserAsync(string userId);
        //Task<List<UserWithRoleDto>> GetAllUsersNonAdminAsync();
    }
}
