using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces.Email;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.InstructorApplication.Commands.RejectInstructorApplication;

public class RejectInstructorApplicationHandler : IRequestHandler<RejectInstructorApplicationCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;

    public RejectInstructorApplicationHandler(
        IApplicationDbContext context,
        INotificationService notificationService,
        IEmailService emailService)
    {
        _context = context;
        _notificationService = notificationService;
        _emailService = emailService;
    }

    public async Task<Result<bool>> Handle(RejectInstructorApplicationCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AdminNotes))
            return Result<bool>.Fail("A reason is required when rejecting an application.");

        var app = await _context.InstructorApplications.FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);
        if (app == null)
            return Result<bool>.Fail("Application not found.");

        if (app.Status != InstructorApplicationStatus.Pending)
            return Result<bool>.Fail("Only pending applications can be rejected.");

        app.Status = InstructorApplicationStatus.Rejected;
        app.AdminNotes = request.AdminNotes;
        app.ReviewedAt = DateTime.UtcNow;
        app.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        await _notificationService.CreateAsync(
            app.ApplicantId!,
            "Instructor Application Update",
            $"Your instructor application was not approved. Reason: {request.AdminNotes}",
            "/InstructorApplication/MyApplication");

        try
        {
            if (!string.IsNullOrWhiteSpace(app.Email))
            {
                await _emailService.SendEmailAsync(
                    subject: "Your Instructor Application — Update",
                    message: $"""
                        <div style="font-family:Arial,sans-serif;max-width:520px">
                          <h2 style="color:#dc3545">Application Not Approved</h2>
                          <p>Hi <strong>{app.FullName}</strong>,</p>
                          <p>Thank you for applying to become an instructor on <strong>Elite Academy</strong>.</p>
                          <p>After review, we were unable to approve your application at this time.</p>
                          <p><strong>Reason:</strong> {request.AdminNotes}</p>
                          <p>You are welcome to apply again after addressing the feedback above.</p>
                          <hr/><p style="color:#888;font-size:12px">Elite Academy</p>
                        </div>
                        """,
                    toEmails: new List<string> { app.Email });
            }
        }
        catch { /* don't fail rejection if email throws */ }

        return Result<bool>.Ok(true, "Application rejected.");
    }
}
