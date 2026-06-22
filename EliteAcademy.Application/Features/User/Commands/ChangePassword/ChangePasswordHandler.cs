using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.User.Commands.ChangePassword;

public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, Result<bool>>
{
    private readonly IUserManager _userManager;
    private readonly IUserContextService _userContextService;

    public ChangePasswordHandler(IUserManager userManager, IUserContextService userContextService)
    {
        _userManager = userManager;
        _userContextService = userContextService;
    }

    public async Task<Result<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _userContextService.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Result<bool>.Fail("User not authenticated.");

        var result = await _userManager.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            return Result<bool>.Fail(result.Errors.FirstOrDefault() ?? "Password change failed.");

        return Result<bool>.Ok(true, "Password changed successfully.");
    }
}
