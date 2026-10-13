using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities.Account;
using MediatR;

namespace EliteAcademy.Application.Features.User.Commands.Login;

public class LoginHandler : IRequestHandler<LoginCommand, Result<string>>
{
    private readonly IUserManager _userManager;
    private readonly ISignInManager _signInManager;
    private readonly IUserContextService _userContextService;
    private readonly IApplicationDbContext _context;

    public LoginHandler(
        IUserManager userManager,
        ISignInManager signInManager,
        IUserContextService userContextService,
        IApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userContextService = userContextService;
        _context = context;
    }

    public async Task<Result<string>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var model = request.Model;

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
        var audit = LoginAudit.Create(
            userId: userId,
            ipAddress: _userContextService.IpAddress,
            userAgent: _userContextService.UserAgent,
            isSuccessful: success,
            errorMessage: errorMessage);

        _context.LoginAudits.Add(audit);

        await _context.SaveChangesAsync();
    }
}
