using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainAnnouncement = EliteAcademy.Domain.Entities.Instructor.Announcement;

namespace EliteAcademy.Application.Features.Announcement.Commands.CreateAnnouncement;

public class CreateAnnouncementHandler : IRequestHandler<CreateAnnouncementCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;
    private readonly INotificationService _notificationService;

    public CreateAnnouncementHandler(
        IApplicationDbContext context,
        IUserContextService userContextService,
        INotificationService notificationService)
    {
        _context             = context;
        _userContextService  = userContextService;
        _notificationService = notificationService;
    }

    public async Task<Result<bool>> Handle(CreateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var instructorId = _userContextService.UserId!;

        var cls = await _context.Classes.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Dto.ClassId, cancellationToken);

        var domainResult = DomainAnnouncement.Create(instructorId, cls, request.Dto.Title, request.Dto.Body);
        if (!domainResult.IsSuccess)
            return Result<bool>.Fail(domainResult.Error);

        _context.Announcements.Add(domainResult.Value!);
        await _context.SaveChangesAsync(cancellationToken);

        var enrollments = await _context.Enrollments.AsNoTracking()
            .Where(e => e.ClassId == request.Dto.ClassId)
            .ToListAsync(cancellationToken);

        foreach (var enrollment in enrollments)
        {
            if (!string.IsNullOrWhiteSpace(enrollment.StudentId))
            {
                await _notificationService.CreateAsync(
                    enrollment.StudentId,
                    $"New announcement: {request.Dto.Title}",
                    $"Your instructor posted an announcement in \"{cls?.ClassName}\".",
                    $"/Student/ClassAnnouncements?classId={request.Dto.ClassId}");
            }
        }

        return Result<bool>.Ok(true, "Announcement posted.");
    }
}
