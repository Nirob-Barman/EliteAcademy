using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.User.Commands.Logout;

public class LogoutHandler : IRequestHandler<LogoutCommand, Result<string>>
{
    private readonly ISignInManager _signInManager;

    public LogoutHandler(ISignInManager signInManager)
    {
        _signInManager = signInManager;
    }

    public async Task<Result<string>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await _signInManager.SignOutAsync();
        return Result<string>.Ok("Success", "Logout successful");
    }
}
