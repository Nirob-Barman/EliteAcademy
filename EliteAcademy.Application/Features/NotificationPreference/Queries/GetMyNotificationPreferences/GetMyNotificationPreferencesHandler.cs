using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Notification;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NotificationPreferenceEntity = EliteAcademy.Domain.Entities.NotificationPreference;

namespace EliteAcademy.Application.Features.NotificationPreference.Queries.GetMyNotificationPreferences;

public class GetMyNotificationPreferencesHandler
    : IRequestHandler<GetMyNotificationPreferencesQuery, Result<NotificationPreferenceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;

    public GetMyNotificationPreferencesHandler(IApplicationDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<Result<NotificationPreferenceDto>> Handle(
        GetMyNotificationPreferencesQuery request, CancellationToken cancellationToken)
    {
        var userId = _userContextService.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Result<NotificationPreferenceDto>.Fail("User not authenticated.");

        var pref = await _context.NotificationPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (pref == null)
            pref = new NotificationPreferenceEntity { UserId = userId };

        return Result<NotificationPreferenceDto>.Ok(new NotificationPreferenceDto
        {
            EmailOnEnrollment = pref.EmailOnEnrollment,
            EmailOnAnnouncement = pref.EmailOnAnnouncement,
            EmailOnClassStatus = pref.EmailOnClassStatus,
            EmailOnApplicationStatus = pref.EmailOnApplicationStatus,
            EmailOnPasswordChange = pref.EmailOnPasswordChange,
            InAppOnEnrollment = pref.InAppOnEnrollment,
            InAppOnAnnouncement = pref.InAppOnAnnouncement,
        });
    }
}
