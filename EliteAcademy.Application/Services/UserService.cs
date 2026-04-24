using EliteAcademy.Application.DTOs.Account;
using EliteAcademy.Application.DTOs.Identity;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Email;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Interfaces.Persistence;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities.Account;

namespace EliteAcademy.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserManager _userManager;
        private readonly ISignInManager _signInManager;
        private readonly IRoleManager _roleManager;
        private readonly IEmailService _emailService;
        private readonly IFileStorage _fileStorage;
        private readonly IUserContextService _userContextService;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUserManager userManager, ISignInManager signInManager, IRoleManager roleManager,
            IEmailService emailService, IFileStorage fileStorage, IUserContextService userContextService,
            IUnitOfWork unitOfWork)
        {
            _userManager        = userManager;
            _signInManager      = signInManager;
            _roleManager        = roleManager;
            _emailService       = emailService;
            _fileStorage        = fileStorage;
            _userContextService = userContextService;
            _unitOfWork         = unitOfWork;
        }


        public async Task<Result<string>> RegisterAsync(RegisterDto model, Stream? imageStream, string? imageFileName)
        {
            if (!string.IsNullOrWhiteSpace(model.Email) && await _userManager.FindByEmailAsync(model.Email) != null)
                return Result<string>.FailField(nameof(model.Email), "This email is already registered.");

            var user = new AppUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth
            };

            if (imageStream != null && !string.IsNullOrWhiteSpace(imageFileName))
                user.ImageUrl = await _fileStorage.UploadFileAsync(imageStream, imageFileName, "uploads/profiles");

            var (succeeded, userId, errors) = await _userManager.CreateAsync(user, model.Password!);

            if (!succeeded)
            {
                var fieldErrors = new Dictionary<string, string>();

                foreach (var error in errors ?? new List<string>())
                {
                    if (error.Contains("email", StringComparison.OrdinalIgnoreCase))
                    {
                        fieldErrors[nameof(model.Email)] = error;
                    }
                    else if (error.Contains("password", StringComparison.OrdinalIgnoreCase))
                    {
                        fieldErrors[nameof(model.Password)] = error;
                    }
                }

                if (fieldErrors.Count > 0)
                {
                    return new Result<string>
                    {
                        Success = false,
                        FieldErrors = fieldErrors,
                        Errors = errors,
                        Message = "Registration failed"
                    };
                }

                return Result<string>.Fail(errors!, "Registration failed");
            }

            var roleResult = await _userManager.AddToRoleAsync(new AppUser { Id = userId }, "Student");

            if (!roleResult.Succeeded)
            {
                await _userManager.RemoveFromRoleAsync(new AppUser { Id = userId }, "Student");
                return Result<string>.Fail("Failed to assign default role to user.");
            }

            //var welcomeMessage = $"Hello {model.FullName},<br>Welcome to CarShop! Thank you for registering.";
            //await _emailService.SendEmailAsync(model.Email!, "Welcome to CarShop", welcomeMessage);

            return Result<string>.Ok(userId, "Registration successful");
        }


        public async Task<Result<string>> LoginAsync(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email!);
            if (user == null)
            {
                await RecordLoginAuditAsync(null, false, "Email not registered.");
                return Result<string>.FailField(nameof(model.Email), "This email is not registered.");
            }

            var isPasswordValid = await _signInManager.CheckPasswordSignInAsync(user, model.Password!);
            if (!isPasswordValid)
            {
                await RecordLoginAuditAsync(user.Id, false, "Incorrect password.");
                return Result<string>.FailField(nameof(model.Password), "Incorrect password.");
            }

            await _signInManager.SignInAsync(user, model.RememberMe);
            await RecordLoginAuditAsync(user.Id, true, null);

            return Result<string>.Ok("Success", "Login successful");
        }

        private async Task RecordLoginAuditAsync(string? userId, bool success, string? errorMessage)
        {
            await _unitOfWork.Repository<LoginAudit>().AddAsync(new LoginAudit
            {
                Id           = Guid.NewGuid(),
                UserId       = userId,
                LoginTime    = DateTime.UtcNow,
                IPAddress    = _userContextService.IpAddress,
                UserAgent    = _userContextService.UserAgent,
                IsSuccessful = success,
                ErrorMessage = errorMessage
            });
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<Result<List<LoginHistoryItemDto>>> GetMyLoginHistoryAsync()
        {
            var userId = _userContextService.UserId;
            if (string.IsNullOrWhiteSpace(userId))
                return Result<List<LoginHistoryItemDto>>.Fail("User not authenticated.");

            var records = await _unitOfWork.Repository<LoginAudit>()
                .GetAllAsync(
                    x => x.UserId == userId,
                    x => new LoginHistoryItemDto
                    {
                        LoginTime    = x.LoginTime,
                        IPAddress    = x.IPAddress,
                        UserAgent    = x.UserAgent,
                        IsSuccessful = x.IsSuccessful,
                        ErrorMessage = x.ErrorMessage
                    });

            var sorted = records.OrderByDescending(x => x.LoginTime).Take(50).ToList();
            return Result<List<LoginHistoryItemDto>>.Ok(sorted);
        }


        public async Task<Result<string>> LogoutAsync()
        {
            await _signInManager.SignOutAsync();
            return Result<string>.Ok("Success", "Logout successful");
        }


        public async Task<bool> CheckPasswordAsync(ApplicationUserDto userDto, string password)
        {
            var user = await _userManager.FindByIdAsync(userDto.Id!);
            return await _userManager.CheckPasswordAsync(user!, password);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email) != null;
        }

        public async Task<Result<EditProfileDto>> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return Result<EditProfileDto>.Fail("User not found.");

            var dto = new EditProfileDto
            {
                //FullName = user.FullName,
                //Address = user.Address
            };

            return Result<EditProfileDto>.Ok(dto);
        }



        public async Task<Result<bool>> UpdateProfileAsync(string userId, EditProfileDto model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result<bool>.Fail("User not found.");

            //user.FullName = model.FullName;
            //user.Address = model.Address;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return Result<bool>.Fail(updateResult.Errors, "Failed to update profile.");
            }

            return Result<bool>.Ok(true, "Profile updated successfully.");
        }


        //public async Task<Result<bool>> ChangePasswordAsync(string userId, ChangePasswordDto model)
        //{
        //    if (string.IsNullOrWhiteSpace(model.CurrentPassword))
        //    {
        //        return Result<bool>.FailField(nameof(model.CurrentPassword), "Password fields cannot be empty.");
        //    }

        //    if (string.IsNullOrWhiteSpace(model.NewPassword))
        //    {
        //        return Result<bool>.FailField(nameof(model.NewPassword), "Password fields cannot be empty.");
        //    }

        //    var user = await _userManager.FindByIdAsync(userId);
        //    if (user == null)
        //        return Result<bool>.Fail("User not found.");

        //    var result = await _userManager.ChangePasswordAsync(user.Id!, model.CurrentPassword, model.NewPassword);

        //    if (!result.Succeeded)
        //    {
        //        return Result<bool>.Fail(result.Errors, "Password change failed.");
        //    }

        //    // Re-sign in to refresh security stamp/cookies
        //    await _signInManager.RefreshSignInAsync(user);

        //    return Result<bool>.Ok(true, "Password changed successfully.");
        //}


        public async Task<Result<bool>> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            var userId = _userContextService.UserId;
            if (string.IsNullOrWhiteSpace(userId))
                return Result<bool>.Fail("User not authenticated.");

            var result = await _userManager.ChangePasswordAsync(userId, currentPassword, newPassword);
            if (!result.Succeeded)
                return Result<bool>.Fail(result.Errors.FirstOrDefault() ?? "Password change failed.");

            return Result<bool>.Ok(true, "Password changed successfully.");
        }

        public async Task<Result<bool>> ForgotPasswordAsync(string email, string callbackUrl)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Result<bool>.Fail("Email is required.");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Result<bool>.Ok(true); // silent — don't reveal if email exists

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var body = $@"
                <div style=""font-family:Arial,sans-serif;max-width:520px;margin:0 auto;"">
                    <h2 style=""color:#1a1a2e;"">&#9971; Elite Academy</h2>
                    <p>Hi {user.FirstName},</p>
                    <p>We received a request to reset your password. Click the button below to choose a new one.</p>
                    <p style=""text-align:center;margin:32px 0;"">
                        <a href=""{callbackUrl}""
                           style=""background:#1a1a2e;color:#fff;padding:12px 28px;border-radius:6px;text-decoration:none;font-weight:600;"">
                            Reset Password
                        </a>
                    </p>
                    <p style=""color:#888;font-size:.85rem;"">This link expires in 24 hours. If you didn't request a password reset, you can safely ignore this email.</p>
                </div>";

            await _emailService.SendEmailAsync(
                subject: "Reset your Elite Academy password",
                message: body,
                toEmails: new List<string> { email });

            return Result<bool>.Ok(true, "Password reset email sent.");
        }

        public async Task<Result<EditProfileDto>> GetMyProfileAsync()
        {
            var user = await _userManager.FindByIdAsync(_userContextService.UserId!);
            if (user == null) return Result<EditProfileDto>.Fail("User not found.");

            var roles = await _userManager.GetRolesAsync(user);

            return Result<EditProfileDto>.Ok(new EditProfileDto
            {
                FirstName   = user.FirstName,
                LastName    = user.LastName,
                Email       = user.Email,
                PhoneNumber = user.PhoneNumber,
                Gender      = user.Gender,
                DateOfBirth = user.DateOfBirth,
                Address     = user.Address,
                ImageUrl    = user.ImageUrl,
                Role        = roles.FirstOrDefault()
            });
        }

        public async Task<Result<bool>> UpdateMyProfileAsync(EditProfileDto dto, Stream? imageStream, string? imageFileName)
        {
            var user = await _userManager.FindByIdAsync(_userContextService.UserId!);
            if (user == null) return Result<bool>.Fail("User not found.");

            user.FirstName   = dto.FirstName;
            user.LastName    = dto.LastName;
            user.PhoneNumber = dto.PhoneNumber;
            user.Gender      = dto.Gender;
            user.DateOfBirth = dto.DateOfBirth;
            user.Address     = dto.Address;

            if (imageStream != null && !string.IsNullOrWhiteSpace(imageFileName))
            {
                if (!string.IsNullOrWhiteSpace(user.ImageUrl))
                    await _fileStorage.DeleteFileAsync(user.ImageUrl);
                user.ImageUrl = await _fileStorage.UploadFileAsync(imageStream, imageFileName, "uploads/profiles");
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return Result<bool>.Fail(result.Errors.FirstOrDefault() ?? "Update failed.");

            return Result<bool>.Ok(true, "Profile updated successfully.");
        }

        public async Task<Result<string>> GeneratePasswordResetTokenAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Result<string>.Fail("Email is required.");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Result<string>.Fail("User not found.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return Result<string>.Ok(token);
        }


        public async Task<Result<bool>> ResetPasswordAsync(string email, string token, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(newPassword))
            {
                return Result<bool>.Fail("Email, token, and new password are required.");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Result<bool>.Fail("User not found.");

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
            {
                return Result<bool>.Fail(result.Errors, "Password reset failed.");
            }

            return Result<bool>.Ok(true, "Password has been reset successfully.");
        }


        public async Task<List<string>> GetAllRolesAsync()
        {
            return await _roleManager.GetAllRolesAsync();
        }


        public async Task<Result<bool>> AssignRoleToUserAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result<bool>.Fail("User not found.");

            // Remove existing roles
            var existingRoles = await _userManager.GetRolesAsync(user);
            var removalResult = await _userManager.RemoveFromRoleAsync(user, existingRoles.FirstOrDefault()!);

            if (!removalResult.Succeeded)
            {
                return Result<bool>.Fail(removalResult.Errors, "Failed to remove existing roles.");
            }

            // Add new role
            var addResult = await _userManager.AddToRoleAsync(user, roleName);
            if (!addResult.Succeeded)
            {
                return Result<bool>.Fail(addResult.Errors, "Failed to assign new role.");
            }

            return Result<bool>.Ok(true, $"Role '{roleName}' assigned successfully.");
        }


        public async Task<Result<List<string>>> GetAllRolesNonAdminAsync()
        {
            try
            {
                var roles = await _roleManager.GetAllRolesAsync(excludeAdmin: true);

                return Result<List<string>>.Ok(roles);
            }
            catch (Exception ex)
            {
                return Result<List<string>>.Fail("Failed to retrieve roles.", ex.Message);
            }
        }


        //public async Task<Result<List<UserWithRoleDto>>> GetAllUsersNonAdminAsync()
        //{
        //    try
        //    {
        //        var allUsers = await _userManager.GetAllUsersAsync();
        //        var nonAdminUsers = new List<UserWithRoleDto>();

        //        foreach (var user in allUsers)
        //        {
        //            bool isAdmin = await _userManager.IsUserInRoleAsync(user, "Admin");
        //            if (isAdmin) continue;

        //            var roles = await _userManager.GetRolesAsync(user);

        //            nonAdminUsers.Add(new UserWithRoleDto
        //            {
        //                UserId = user.Id!,
        //                Email = user.Email!,
        //                FullName = user.FullName!,
        //                Address = user.Address,
        //                CurrentRole = roles.FirstOrDefault() ?? "None"
        //            });
        //        }

        //        return Result<List<UserWithRoleDto>>.Ok(nonAdminUsers);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Result<List<UserWithRoleDto>>.Fail("An error occurred while fetching users.", ex.Message);
        //    }
        //}


        public async Task<Result<bool>> CreateRoleAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return Result<bool>.Fail("Role name is required.");

            roleName = roleName.Trim();

            if (await _roleManager.RoleExistsAsync(roleName))
                return Result<bool>.Fail($"Role '{roleName}' already exists.");

            var result = await _roleManager.CreateRoleAsync(roleName);

            if (!result.Succeeded)
            {
                return Result<bool>.Fail(result.Errors, "Failed to create role.");
            }

            return Result<bool>.Ok(true, $"Role '{roleName}' created successfully.");
        }
    }
}
