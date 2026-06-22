using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.User.Queries.GeneratePasswordResetToken;

public class GeneratePasswordResetTokenHandler : IRequestHandler<GeneratePasswordResetTokenQuery, Result<string>>
{
    private readonly IUserManager _userManager;

    public GeneratePasswordResetTokenHandler(IUserManager userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<string>> Handle(GeneratePasswordResetTokenQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return Result<string>.Fail("Email is required.");

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result<string>.Fail("User not found.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        return Result<string>.Ok(token);
    }
}
