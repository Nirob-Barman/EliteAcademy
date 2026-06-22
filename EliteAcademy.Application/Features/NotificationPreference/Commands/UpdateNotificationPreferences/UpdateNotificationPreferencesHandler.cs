using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NotificationPreferenceEntity = EliteAcademy.Domain.Entities.NotificationPreference;

namespace EliteAcademy.Application.Features.NotificationPreference.Commands.UpdateNotificationPreferences;

public class UpdateNotificationPreferencesHandler
    : IRequestHandler<UpdateNotificationPreferencesCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserContextService _userContextService;

    public UpdateNotificationPreferencesHandler(IApplicationDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<Result<bool>> Handle(UpdateNotificationPreferencesCommand request, CancellationToken cancellationToken)
    {
        var userId = _userContextService.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Result<bool>.Fail("User not authenticated.");

        var dto = request.Dto;
        var pref = await _context.NotificationPreferences
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (pref == null)
        {
            pref = new NotificationPreferenceEntity { UserId = userId };
            pref.EmailOnEnrollment = dto.EmailOnEnrollment;
            pref.EmailOnAnnouncement = dto.EmailOnAnnouncement;
            pref.EmailOnClassStatus = dto.EmailOnClassStatus;
            pref.EmailOnApplicationStatus = dto.EmailOnApplicationStatus;
            pref.EmailOnPasswordChange = dto.EmailOnPasswordChange;
            pref.InAppOnEnrollment = dto.InAppOnEnrollment;
            pref.InAppOnAnnouncement = dto.InAppOnAnnouncement;
            _context.NotificationPreferences.Add(pref);
        }
        else
        {
            pref.EmailOnEnrollment = dto.EmailOnEnrollment;
            pref.EmailOnAnnouncement = dto.EmailOnAnnouncement;
            pref.EmailOnClassStatus = dto.EmailOnClassStatus;
            pref.EmailOnApplicationStatus = dto.EmailOnApplicationStatus;
            pref.EmailOnPasswordChange = dto.EmailOnPasswordChange;
            pref.InAppOnEnrollment = dto.InAppOnEnrollment;
            pref.InAppOnAnnouncement = dto.InAppOnAnnouncement;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Ok(true, "Preferences saved.");
    }
}
