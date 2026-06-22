using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.User.Commands.ResetPassword;

public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, Result<bool>>
{
    private readonly IUserManager _userManager;

    public ResetPasswordHandler(IUserManager userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
            return Result<bool>.Fail("Email, token, and new password are required.");

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result<bool>.Fail("User not found.");

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
            return Result<bool>.Fail(result.Errors, "Password reset failed.");

        return Result<bool>.Ok(true, "Password has been reset successfully.");
    }
}
