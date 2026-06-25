using EliteAcademy.Application.Interfaces.Email;
using EliteAcademy.Application.Interfaces.Identity;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Domain.Events;
using MediatR;

namespace EliteAcademy.Application.Features.Admin.Events;

public class ClassApprovedEventHandler : INotificationHandler<ClassApprovedEvent>
{
    private readonly INotificationService _notifications;
    private readonly IAuditLogService _audit;
    private readonly IEmailService _email;
    private readonly IUserManager _userManager;

    public ClassApprovedEventHandler(
        INotificationService notifications,
        IAuditLogService audit,
        IEmailService email,
        IUserManager userManager)
    {
        _notifications = notifications;
        _audit         = audit;
        _email         = email;
        _userManager   = userManager;
    }

    public async Task Handle(ClassApprovedEvent notification, CancellationToken cancellationToken)
    {
        await _notifications.CreateAsync(
            notification.InstructorId,
            "Class Approved",
            $"Your class \"{notification.ClassName}\" has been approved and is now live.",
            "/Instructor/MyClasses");

        await _audit.LogAsync("Class", "Approve",
            details: $"Approved class \"{notification.ClassName}\" (ID: {notification.ClassId})");

        try
        {
            var instructor = await _userManager.FindByIdAsync(notification.InstructorId);
            if (!string.IsNullOrWhiteSpace(instructor?.Email))
            {
                await _email.SendEmailAsync(
                    subject: $"Class Approved — {notification.ClassName}",
                    message: $"""
                        <div style="font-family:Arial,sans-serif;max-width:520px">
                          <h2 style="color:#198754">Your class has been approved!</h2>
                          <p>Hi <strong>{instructor.FirstName}</strong>,</p>
                          <p>Your class <strong>{notification.ClassName}</strong> has been approved and is now live for students to enroll.</p>
                          <p>Visit your <a href="/Instructor/MyClasses">My Classes</a> page to manage it.</p>
                          <hr/><p style="color:#888;font-size:12px">Elite Academy</p>
                        </div>
                        """,
                    toEmails: new List<string> { instructor.Email });
            }
        }
        catch { }
    }
}
