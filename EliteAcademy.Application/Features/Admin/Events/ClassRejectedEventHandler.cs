using EliteAcademy.Application.Interfaces.Email;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Domain.Events;
using MediatR;

namespace EliteAcademy.Application.Features.Admin.Events;

public class ClassRejectedEventHandler : INotificationHandler<ClassRejectedEvent>
{
    private readonly INotificationService _notifications;
    private readonly IAuditLogService _audit;
    private readonly IEmailService _email;
    private readonly IUserManager _userManager;

    public ClassRejectedEventHandler(
        INotificationService notifications,
        IAuditLogService audit,
        IEmailService email,
        IUserManager userManager)
    {
        _notifications = notifications;
        _audit = audit;
        _email = email;
        _userManager = userManager;
    }

    public async Task Handle(ClassRejectedEvent notification, CancellationToken cancellationToken)
    {
        await _notifications.CreateAsync(
            notification.InstructorId,
            "Class Rejected",
            $"Your class \"{notification.ClassName}\" was not approved. Feedback: {notification.Feedback}",
            "/Instructor/MyClasses");

        await _audit.LogAsync("Class", "Reject",
            details: $"Rejected class \"{notification.ClassName}\" (ID: {notification.ClassId}). Feedback: {notification.Feedback}");

        try
        {
            var instructor = await _userManager.FindByIdAsync(notification.InstructorId);
            if (!string.IsNullOrWhiteSpace(instructor?.Email))
            {
                await _email.SendEmailAsync(
                    subject: $"Class Not Approved — {notification.ClassName}",
                    message: $"""
                        <div style="font-family:Arial,sans-serif;max-width:520px">
                          <h2 style="color:#dc3545">Class Not Approved</h2>
                          <p>Hi <strong>{instructor.FirstName}</strong>,</p>
                          <p>Your class <strong>{notification.ClassName}</strong> was not approved.</p>
                          <p><strong>Feedback:</strong> {notification.Feedback}</p>
                          <p>Please review your class and resubmit after making the necessary changes.</p>
                          <hr/><p style="color:#888;font-size:12px">Elite Academy</p>
                        </div>
                        """,
                    toEmails: new List<string> { instructor.Email });
            }
        }
        catch { }
    }
}
