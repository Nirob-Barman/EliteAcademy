using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Domain.Entities.Account;
using EliteAcademy.Infrastructure.Identity.Entity;
using Microsoft.AspNetCore.Identity;

namespace EliteAcademy.Infrastructure.Identity
{
    public class IdentitySignInManager : ISignInManager
    {
        private readonly SignInManager<ApplicationIdentityUser> _signInManager;

        public IdentitySignInManager(SignInManager<ApplicationIdentityUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<bool> CheckPasswordSignInAsync(AppUser user, string password)
        {
            var identityUser = await _signInManager.UserManager.FindByIdAsync(user.Id!.ToString());
            if (identityUser == null) return false;

            var result = await _signInManager.CheckPasswordSignInAsync(identityUser, password, false);
            return result.Succeeded;
        }

        public async Task SignInAsync(AppUser user, bool isPersistent)
        {
            var identityUser = await _signInManager.UserManager.FindByIdAsync(user.Id!.ToString());
            if (identityUser != null)
            {
                await _signInManager.SignInAsync(identityUser, isPersistent);
            }
        }


        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task RefreshSignInAsync(AppUser user)
        {
            var identityUser = await _signInManager.UserManager.FindByIdAsync(user.Id!.ToString());
            if (identityUser != null)
            {
                await _signInManager.RefreshSignInAsync(identityUser);
            }
        }
    }
}
