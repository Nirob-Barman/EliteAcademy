using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Features.Announcement.Events;

public class AnnouncementPostedEventHandler : INotificationHandler<AnnouncementPostedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notifications;

    public AnnouncementPostedEventHandler(
        IApplicationDbContext context,
        INotificationService notifications)
    {
        _context = context;
        _notifications = notifications;
    }

    public async Task Handle(AnnouncementPostedEvent notification, CancellationToken cancellationToken)
    {
        var enrollments = await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.ClassId == notification.ClassId)
            .ToListAsync(cancellationToken);

        foreach (var enrollment in enrollments)
        {
            if (!string.IsNullOrWhiteSpace(enrollment.StudentId))
            {
                await _notifications.CreateAsync(
                    enrollment.StudentId,
                    $"New announcement: {notification.Title}",
                    $"Your instructor posted an announcement in \"{notification.ClassName}\".",
                    $"/Student/ClassAnnouncements?classId={notification.ClassId}");
            }
        }
    }
}
