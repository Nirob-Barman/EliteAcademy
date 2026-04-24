using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Domain.Entities.Account;
using EliteAcademy.Infrastructure.Identity.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Infrastructure.Identity
{
    public class IdentityUserManager : IUserManager
    {
        private readonly UserManager<ApplicationIdentityUser> _userManager;

        public IdentityUserManager(UserManager<ApplicationIdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<(bool Succeeded, string? UserId, List<string> Errors)> CreateAsync(AppUser user, string password)
        {
            var identityUser = new ApplicationIdentityUser
            {
                Email           = user.Email,
                UserName        = user.Email,
                PhoneNumber     = user.PhoneNumber,
                FirstName       = user.FirstName,
                LastName        = user.LastName,
                Address         = user.Address,
                Gender          = user.Gender,
                DateOfBirth     = user.DateOfBirth,
                ImageUrl        = user.ImageUrl,
                IsAgreedToTerms = user.IsAgreedToTerms
            };

            var result = await _userManager.CreateAsync(identityUser, password);
            return result.Succeeded
                ? (true, identityUser.Id, new List<string>())
                : (false, null, result.Errors.Select(e => e.Description).ToList());
        }

        public async Task<(bool Succeeded, List<string> Errors)> UpdateAsync(AppUser user)
        {
            var identityUser = await _userManager.FindByIdAsync(user.Id!);
            if (identityUser == null)
                return (false, new List<string> { "User not found." });

            identityUser.FirstName      = user.FirstName;
            identityUser.LastName       = user.LastName;
            identityUser.PhoneNumber    = user.PhoneNumber;
            identityUser.Address        = user.Address;
            identityUser.Gender         = user.Gender;
            identityUser.DateOfBirth    = user.DateOfBirth;
            identityUser.ImageUrl       = user.ImageUrl;
            identityUser.IsAgreedToTerms = user.IsAgreedToTerms;

            var result = await _userManager.UpdateAsync(identityUser);
            return (result.Succeeded, result.Errors.Select(e => e.Description).ToList());
        }

        public async Task<(bool Succeeded, List<string> Errors)> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return (false, new List<string> { "User not found." });

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            return (result.Succeeded, result.Errors.Select(e => e.Description).ToList());
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new Exception("User not found.");
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<AppUser?> FindByEmailAsync(string email)
        {
            var u = await _userManager.FindByEmailAsync(email);
            if (u == null) return null;
            return MapToAppUser(u);
        }

        public async Task<AppUser?> FindByIdAsync(string id)
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u == null) return null;
            return MapToAppUser(u);
        }

        public async Task<string[]> GetRolesAsync(AppUser user)
        {
            var identityUser = await _userManager.FindByIdAsync(user.Id!);
            if (identityUser == null) return Array.Empty<string>();
            return (await _userManager.GetRolesAsync(identityUser)).ToArray();
        }

        public async Task<bool> CheckPasswordAsync(AppUser user, string password)
        {
            var identityUser = await _userManager.FindByIdAsync(user.Id!);
            if (identityUser == null) return false;
            return await _userManager.CheckPasswordAsync(identityUser, password);
        }

        public async Task<string> GeneratePasswordResetTokenAsync(AppUser user)
        {
            var identityUser = await _userManager.FindByIdAsync(user.Id!);
            if (identityUser == null) return string.Empty;
            return await _userManager.GeneratePasswordResetTokenAsync(identityUser);
        }

        public async Task<(bool Succeeded, List<string> Errors)> ResetPasswordAsync(AppUser user, string token, string newPassword)
        {
            var identityUser = await _userManager.FindByIdAsync(user.Id!);
            if (identityUser == null)
                return (false, new List<string> { "User not found." });
            var result = await _userManager.ResetPasswordAsync(identityUser, token, newPassword);
            return (result.Succeeded, result.Errors.Select(e => e.Description).ToList());
        }

        public async Task<(bool Succeeded, List<string> Errors)> AddToRoleAsync(AppUser user, string roleName)
        {
            var identityUser = await _userManager.FindByIdAsync(user.Id!);
            if (identityUser == null)
                return (false, new List<string> { "User not found." });
            var result = await _userManager.AddToRoleAsync(identityUser, roleName);
            return (result.Succeeded, result.Errors.Select(e => e.Description).ToList());
        }

        public async Task<(bool Succeeded, List<string> Errors)> RemoveFromRoleAsync(AppUser user, string roleName)
        {
            var identityUser = await _userManager.FindByIdAsync(user.Id!);
            if (identityUser == null)
                return (false, new List<string> { "User not found." });
            var result = await _userManager.RemoveFromRoleAsync(identityUser, roleName);
            return (result.Succeeded, result.Errors.Select(e => e.Description).ToList());
        }

        public async Task<IEnumerable<AppUser>> GetAllUsersAsync()
        {
            var identityUsers = await _userManager.Users.ToListAsync();
            return identityUsers.Select(MapToAppUser).ToList();
        }

        public async Task<IEnumerable<AppUser>> GetUsersByRoleAsync(string role)
        {
            var identityUsers = await _userManager.GetUsersInRoleAsync(role);
            return identityUsers.Select(MapToAppUser).ToList();
        }

        public async Task<bool> IsUserInRoleAsync(AppUser user, string role)
        {
            var identityUser = await _userManager.FindByIdAsync(user.Id!);
            if (identityUser == null) return false;
            return await _userManager.IsInRoleAsync(identityUser, role);
        }

        public async Task<(bool Succeeded, List<string> Errors)> BanUserAsync(string userId)
        {
            var identityUser = await _userManager.FindByIdAsync(userId);
            if (identityUser == null)
                return (false, new List<string> { "User not found." });

            identityUser.LockoutEnabled = true;
            identityUser.LockoutEnd     = DateTimeOffset.MaxValue;
            var result = await _userManager.UpdateAsync(identityUser);
            return (result.Succeeded, result.Errors.Select(e => e.Description).ToList());
        }

        public async Task<(bool Succeeded, List<string> Errors)> UnbanUserAsync(string userId)
        {
            var identityUser = await _userManager.FindByIdAsync(userId);
            if (identityUser == null)
                return (false, new List<string> { "User not found." });

            identityUser.LockoutEnd = null;
            var result = await _userManager.UpdateAsync(identityUser);
            return (result.Succeeded, result.Errors.Select(e => e.Description).ToList());
        }

        private static AppUser MapToAppUser(ApplicationIdentityUser u) => new()
        {
            Id              = u.Id,
            Email           = u.Email!,
            FirstName       = u.FirstName,
            LastName        = u.LastName,
            PhoneNumber     = u.PhoneNumber,
            DateOfBirth     = u.DateOfBirth,
            Gender          = u.Gender,
            Address         = u.Address,
            ImageUrl        = u.ImageUrl,
            IsAgreedToTerms = u.IsAgreedToTerms,
            IsBanned        = u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow
        };
    }
}
