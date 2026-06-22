using EliteAcademy.Application.Interfaces.Email;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.User.Commands.ForgotPassword;

public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, Result<bool>>
{
    private readonly IUserManager _userManager;
    private readonly IEmailService _emailService;

    public ForgotPasswordHandler(IUserManager userManager, IEmailService emailService)
    {
        _userManager = userManager;
        _emailService = emailService;
    }

    public async Task<Result<bool>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return Result<bool>.Fail("Email is required.");

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result<bool>.Ok(true);

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var body = $@"
            <div style=""font-family:Arial,sans-serif;max-width:520px;margin:0 auto;"">
                <h2 style=""color:#1a1a2e;"">&#9971; Elite Academy</h2>
                <p>Hi {user.FirstName},</p>
                <p>We received a request to reset your password. Click the button below to choose a new one.</p>
                <p style=""text-align:center;margin:32px 0;"">
                    <a href=""{request.CallbackUrl}""
                       style=""background:#1a1a2e;color:#fff;padding:12px 28px;border-radius:6px;text-decoration:none;font-weight:600;"">
                        Reset Password
                    </a>
                </p>
                <p style=""color:#888;font-size:.85rem;"">This link expires in 24 hours. If you didn't request a password reset, you can safely ignore this email.</p>
            </div>";

        await _emailService.SendEmailAsync(
            subject: "Reset your Elite Academy password",
            message: body,
            toEmails: new List<string> { request.Email });

        return Result<bool>.Ok(true, "Password reset email sent.");
    }
}
