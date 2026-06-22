using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces.Email;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.InstructorApplication.Commands.ApproveInstructorApplication;

public class ApproveInstructorApplicationHandler : IRequestHandler<ApproveInstructorApplicationCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserManager _userManager;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;

    public ApproveInstructorApplicationHandler(
        IApplicationDbContext context,
        IUserManager userManager,
        INotificationService notificationService,
        IEmailService emailService)
    {
        _context = context;
        _userManager = userManager;
        _notificationService = notificationService;
        _emailService = emailService;
    }

    public async Task<Result<bool>> Handle(ApproveInstructorApplicationCommand request, CancellationToken cancellationToken)
    {
        var app = await _context.InstructorApplications.FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);
        if (app == null)
            return Result<bool>.Fail("Application not found.");

        if (app.Status != InstructorApplicationStatus.Pending)
            return Result<bool>.Fail("Only pending applications can be approved.");

        var user = await _userManager.FindByIdAsync(app.ApplicantId!);
        if (user == null)
            return Result<bool>.Fail("Applicant account not found.");

        var currentRoles = await _userManager.GetRolesAsync(user);
        foreach (var role in currentRoles)
            await _userManager.RemoveFromRoleAsync(user, role);

        var addResult = await _userManager.AddToRoleAsync(user, "Instructor");
        if (!addResult.Succeeded)
            return Result<bool>.Fail(addResult.Errors.FirstOrDefault() ?? "Failed to assign Instructor role.");

        app.Status = InstructorApplicationStatus.Approved;
        app.ReviewedAt = DateTime.UtcNow;
        app.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        await _notificationService.CreateAsync(
            app.ApplicantId!,
            "Instructor Application Approved",
            "Congratulations! Your instructor application has been approved. You can now create classes.",
            "/Instructor/Dashboard");

        try
        {
            if (!string.IsNullOrWhiteSpace(app.Email))
            {
                await _emailService.SendEmailAsync(
                    subject: "Your Instructor Application — Approved!",
                    message: $"""
                        <div style="font-family:Arial,sans-serif;max-width:520px">
                          <h2 style="color:#198754">Application Approved!</h2>
                          <p>Hi <strong>{app.FullName}</strong>,</p>
                          <p>Great news — your application to become an instructor on <strong>Elite Academy</strong> has been approved.</p>
                          <p>You can now log in and start creating classes from your <strong>Instructor Dashboard</strong>.</p>
                          <p>Note: you may need to log out and log back in for the role change to take effect.</p>
                          <hr/><p style="color:#888;font-size:12px">Elite Academy</p>
                        </div>
                        """,
                    toEmails: new List<string> { app.Email });
            }
        }
        catch { /* don't fail approval if email throws */ }

        return Result<bool>.Ok(true, $"{app.FullName}'s application approved. They are now an Instructor.");
    }
}
