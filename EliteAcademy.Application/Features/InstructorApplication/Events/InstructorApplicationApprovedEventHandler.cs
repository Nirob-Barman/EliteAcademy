using EliteAcademy.Application.Interfaces.Email;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Domain.Events;
using MediatR;

namespace EliteAcademy.Application.Features.InstructorApplication.Events;

public class InstructorApplicationApprovedEventHandler : INotificationHandler<InstructorApplicationApprovedEvent>
{
    private readonly INotificationService _notifications;
    private readonly IEmailService _email;

    public InstructorApplicationApprovedEventHandler(
        INotificationService notifications,
        IEmailService email)
    {
        _notifications = notifications;
        _email = email;
    }

    public async Task Handle(InstructorApplicationApprovedEvent notification, CancellationToken cancellationToken)
    {
        await _notifications.CreateAsync(
            notification.ApplicantId,
            "Instructor Application Approved",
            "Congratulations! Your instructor application has been approved. You can now create classes.",
            "/Instructor/Dashboard");

        try
        {
            if (!string.IsNullOrWhiteSpace(notification.Email))
            {
                await _email.SendEmailAsync(
                    subject: "Your Instructor Application — Approved!",
                    message: $"""
                        <div style="font-family:Arial,sans-serif;max-width:520px">
                          <h2 style="color:#198754">Application Approved!</h2>
                          <p>Hi <strong>{notification.FullName}</strong>,</p>
                          <p>Great news — your application to become an instructor on <strong>Elite Academy</strong> has been approved.</p>
                          <p>You can now log in and start creating classes from your <strong>Instructor Dashboard</strong>.</p>
                          <p>Note: you may need to log out and log back in for the role change to take effect.</p>
                          <hr/><p style="color:#888;font-size:12px">Elite Academy</p>
                        </div>
                        """,
                    toEmails: new List<string> { notification.Email });
            }
        }
        catch { }
    }
}
