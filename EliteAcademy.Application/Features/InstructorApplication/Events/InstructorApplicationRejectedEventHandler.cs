using EliteAcademy.Application.Interfaces.Email;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Domain.Events;
using MediatR;

namespace EliteAcademy.Application.Features.InstructorApplication.Events;

public class InstructorApplicationRejectedEventHandler : INotificationHandler<InstructorApplicationRejectedEvent>
{
    private readonly INotificationService _notifications;
    private readonly IEmailService _email;

    public InstructorApplicationRejectedEventHandler(
        INotificationService notifications,
        IEmailService email)
    {
        _notifications = notifications;
        _email         = email;
    }

    public async Task Handle(InstructorApplicationRejectedEvent notification, CancellationToken cancellationToken)
    {
        await _notifications.CreateAsync(
            notification.ApplicantId,
            "Instructor Application Update",
            $"Your instructor application was not approved. Reason: {notification.AdminNotes}",
            "/InstructorApplication/MyApplication");

        try
        {
            if (!string.IsNullOrWhiteSpace(notification.Email))
            {
                await _email.SendEmailAsync(
                    subject: "Your Instructor Application — Update",
                    message: $"""
                        <div style="font-family:Arial,sans-serif;max-width:520px">
                          <h2 style="color:#dc3545">Application Not Approved</h2>
                          <p>Hi <strong>{notification.FullName}</strong>,</p>
                          <p>Thank you for applying to become an instructor on <strong>Elite Academy</strong>.</p>
                          <p>After review, we were unable to approve your application at this time.</p>
                          <p><strong>Reason:</strong> {notification.AdminNotes}</p>
                          <p>You are welcome to apply again after addressing the feedback above.</p>
                          <hr/><p style="color:#888;font-size:12px">Elite Academy</p>
                        </div>
                        """,
                    toEmails: new List<string> { notification.Email });
            }
        }
        catch { }
    }
}
