using EliteAcademy.Application.DTOs.Notification;
using EliteAcademy.Application.Features.NotificationPreference.Commands.UpdateNotificationPreferences;
using EliteAcademy.Application.Features.NotificationPreference.Queries.GetMyNotificationPreferences;
using EliteAcademy.Application.Features.User.Commands.ChangePassword;
using EliteAcademy.Application.Features.User.Commands.ForgotPassword;
using EliteAcademy.Application.Features.User.Commands.Login;
using EliteAcademy.Application.Features.User.Commands.Logout;
using EliteAcademy.Application.Features.User.Commands.Register;
using EliteAcademy.Application.Features.User.Commands.ResetPassword;
using EliteAcademy.Application.Features.User.Commands.UpdateMyProfile;
using EliteAcademy.Application.Features.User.Queries.GeneratePasswordResetToken;
using EliteAcademy.Application.Features.User.Queries.GetMyLoginHistory;
using EliteAcademy.Application.Features.User.Queries.GetMyProfile;
using EliteAcademy.Web.ViewModels.Account;
using EliteAcademy.Web.ViewModels.Mappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EliteAcademy.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IMediator _mediator;

        public AccountController(IMediator mediator)
        {
            _mediator = mediator;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var dto = AccountMapper.ToDto(model);
            Stream? stream = null;
            string? fileName = null;

            if (model.Photo != null && model.Photo.Length > 0)
            {
                stream = model.Photo.OpenReadStream();
                fileName = model.Photo.FileName;
            }

            var result = await _mediator.Send(new RegisterCommand(dto, stream, fileName));


            if (!result.Success)
            {
                if (result.FieldErrors != null && result.FieldErrors.Any())
                {
                    foreach (var kvp in result.FieldErrors)
                        ModelState.AddModelError(kvp.Key, kvp.Value);
                }
                else
                {
                    TempData["ErrorMessage"] = result.Errors?.FirstOrDefault() ?? result.Message ?? "Registration failed. Please try again.";
                }

                return View(model);
            }

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl; // Store the return URL for redirection after login
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            var dto = AccountMapper.ToDto(model);
            var result = await _mediator.Send(new LoginCommand(dto));

            if (!result.Success)
            {
                if (result.FieldErrors != null && result.FieldErrors.Any())
                {
                    foreach (var kvp in result.FieldErrors)
                        ModelState.AddModelError(kvp.Key, kvp.Value);
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message ?? "Invalid email or password.";
                }

                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            return RedirectToLocal(returnUrl);
        }


        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _mediator.Send(new ChangePasswordCommand(vm.CurrentPassword!, vm.NewPassword!));
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message ?? "Password change failed.";
                return View(vm);
            }

            TempData["SuccessMessage"] = "Password changed successfully.";
            return RedirectToAction(nameof(ChangePassword));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var callbackUrl = Url.Action("ResetPassword", "Account",
                values: null, protocol: Request.Scheme)!;

            var tokenResult = await _mediator.Send(new GeneratePasswordResetTokenQuery(model.Email!));
            if (tokenResult.Success)
            {
                var resetUrl = Url.Action("ResetPassword", "Account",
                    new { email = model.Email, token = tokenResult.Data },
                    protocol: Request.Scheme)!;

                await _mediator.Send(new ForgotPasswordCommand(model.Email!, resetUrl));
            }

            TempData["SuccessMessage"] = "If that email is registered, a reset link has been sent.";
            return RedirectToAction("ForgotPassword");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string? email, string? token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            {
                TempData["ErrorMessage"] = "Invalid password reset link.";
                return RedirectToAction("Login");
            }
            return View(new ResetPasswordViewModel { Email = email, Token = token });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _mediator.Send(new ResetPasswordCommand(model.Email!, model.Token!, model.Password!));
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message ?? "Password reset failed.";
                return View(model);
            }

            TempData["SuccessMessage"] = "Password reset successfully. You can now sign in.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var result = await _mediator.Send(new GetMyProfileQuery());
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Index", "Dashboard");
            }

            var dto = result.Data!;
            return View(new ProfileViewModel
            {
                FirstName        = dto.FirstName ?? string.Empty,
                LastName         = dto.LastName  ?? string.Empty,
                Email            = dto.Email,
                PhoneNumber      = dto.PhoneNumber,
                Gender           = dto.Gender,
                DateOfBirth      = dto.DateOfBirth,
                Address          = dto.Address,
                ExistingPhotoUrl = dto.ImageUrl,
                Role             = dto.Role
            });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var dto = new Application.DTOs.Identity.EditProfileDto
            {
                FirstName   = vm.FirstName,
                LastName    = vm.LastName,
                PhoneNumber = vm.PhoneNumber,
                Gender      = vm.Gender,
                DateOfBirth = vm.DateOfBirth,
                Address     = vm.Address
            };

            Stream? stream   = null;
            string? fileName = null;
            if (vm.PhotoFile != null && vm.PhotoFile.Length > 0)
            {
                stream   = vm.PhotoFile.OpenReadStream();
                fileName = vm.PhotoFile.FileName;
            }

            var result = await _mediator.Send(new UpdateMyProfileCommand(dto, stream, fileName));
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message ?? "Update failed.";
                return View(vm);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> LoginHistory()
        {
            var result = await _mediator.Send(new GetMyLoginHistoryQuery());
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Profile));
            }
            return View(result.Data);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> NotificationPreferences()
        {
            var result = await _mediator.Send(new GetMyNotificationPreferencesQuery());
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Profile));
            }
            return View(new NotificationPreferencesViewModel
            {
                EmailOnEnrollment        = result.Data!.EmailOnEnrollment,
                EmailOnAnnouncement      = result.Data.EmailOnAnnouncement,
                EmailOnClassStatus       = result.Data.EmailOnClassStatus,
                EmailOnApplicationStatus = result.Data.EmailOnApplicationStatus,
                EmailOnPasswordChange    = result.Data.EmailOnPasswordChange,
                InAppOnEnrollment        = result.Data.InAppOnEnrollment,
                InAppOnAnnouncement      = result.Data.InAppOnAnnouncement,
            });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NotificationPreferences(NotificationPreferencesViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _mediator.Send(new UpdateNotificationPreferencesCommand(new NotificationPreferenceDto
            {
                EmailOnEnrollment        = vm.EmailOnEnrollment,
                EmailOnAnnouncement      = vm.EmailOnAnnouncement,
                EmailOnClassStatus       = vm.EmailOnClassStatus,
                EmailOnApplicationStatus = vm.EmailOnApplicationStatus,
                EmailOnPasswordChange    = vm.EmailOnPasswordChange,
                InAppOnEnrollment        = vm.InAppOnEnrollment,
                InAppOnAnnouncement      = vm.InAppOnAnnouncement,
            }));

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return View(vm);
            }

            TempData["SuccessMessage"] = "Preferences saved.";
            return RedirectToAction(nameof(NotificationPreferences));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _mediator.Send(new LogoutCommand());
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied(string? returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
    }
}
