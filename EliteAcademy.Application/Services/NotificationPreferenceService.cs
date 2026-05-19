using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Notification;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities;

namespace EliteAcademy.Application.Services
{
    public class NotificationPreferenceService : INotificationPreferenceService
    {
        private readonly IApplicationDbContext _context;
        private readonly IAsyncQueryExecutor _executor;
        private readonly IUserContextService _userContextService;

        public NotificationPreferenceService(
            IApplicationDbContext context,
            IAsyncQueryExecutor executor,
            IUserContextService userContextService)
        {
            _context            = context;
            _executor           = executor;
            _userContextService = userContextService;
        }

        public async Task<Result<NotificationPreferenceDto>> GetMyPreferencesAsync()
        {
            var userId = _userContextService.UserId;
            if (string.IsNullOrWhiteSpace(userId))
                return Result<NotificationPreferenceDto>.Fail("User not authenticated.");

            var pref = await _executor.FirstOrDefaultAsync(_context.NotificationPreferences.Where(x => x.UserId == userId));

            if (pref == null)
                pref = new NotificationPreference { UserId = userId };

            return Result<NotificationPreferenceDto>.Ok(new NotificationPreferenceDto
            {
                EmailOnEnrollment        = pref.EmailOnEnrollment,
                EmailOnAnnouncement      = pref.EmailOnAnnouncement,
                EmailOnClassStatus       = pref.EmailOnClassStatus,
                EmailOnApplicationStatus = pref.EmailOnApplicationStatus,
                EmailOnPasswordChange    = pref.EmailOnPasswordChange,
                InAppOnEnrollment        = pref.InAppOnEnrollment,
                InAppOnAnnouncement      = pref.InAppOnAnnouncement,
            });
        }

        public async Task<Result<bool>> UpdateMyPreferencesAsync(NotificationPreferenceDto dto)
        {
            var userId = _userContextService.UserId;
            if (string.IsNullOrWhiteSpace(userId))
                return Result<bool>.Fail("User not authenticated.");

            var pref = await _executor.FirstOrDefaultAsync(_context.NotificationPreferences.Where(x => x.UserId == userId));

            if (pref == null)
            {
                pref = new NotificationPreference { UserId = userId };
                pref.EmailOnEnrollment        = dto.EmailOnEnrollment;
                pref.EmailOnAnnouncement      = dto.EmailOnAnnouncement;
                pref.EmailOnClassStatus       = dto.EmailOnClassStatus;
                pref.EmailOnApplicationStatus = dto.EmailOnApplicationStatus;
                pref.EmailOnPasswordChange    = dto.EmailOnPasswordChange;
                pref.InAppOnEnrollment        = dto.InAppOnEnrollment;
                pref.InAppOnAnnouncement      = dto.InAppOnAnnouncement;
                _context.Add(pref);
            }
            else
            {
                pref.EmailOnEnrollment        = dto.EmailOnEnrollment;
                pref.EmailOnAnnouncement      = dto.EmailOnAnnouncement;
                pref.EmailOnClassStatus       = dto.EmailOnClassStatus;
                pref.EmailOnApplicationStatus = dto.EmailOnApplicationStatus;
                pref.EmailOnPasswordChange    = dto.EmailOnPasswordChange;
                pref.InAppOnEnrollment        = dto.InAppOnEnrollment;
                pref.InAppOnAnnouncement      = dto.InAppOnAnnouncement;
            }

            await _context.SaveChangesAsync();
            return Result<bool>.Ok(true, "Preferences saved.");
        }
    }
}
