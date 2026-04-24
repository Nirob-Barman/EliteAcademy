using EliteAcademy.Application.DTOs.Notification;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Persistence;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities;

namespace EliteAcademy.Application.Services
{
    public class NotificationPreferenceService : INotificationPreferenceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContextService _userContextService;

        public NotificationPreferenceService(IUnitOfWork unitOfWork, IUserContextService userContextService)
        {
            _unitOfWork         = unitOfWork;
            _userContextService = userContextService;
        }

        public async Task<Result<NotificationPreferenceDto>> GetMyPreferencesAsync()
        {
            var userId = _userContextService.UserId;
            if (string.IsNullOrWhiteSpace(userId))
                return Result<NotificationPreferenceDto>.Fail("User not authenticated.");

            var pref = await _unitOfWork.Repository<NotificationPreference>()
                .FirstOrDefaultAsync(x => x.UserId == userId);

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

            var pref = await _unitOfWork.Repository<NotificationPreference>()
                .FirstOrDefaultAsync(x => x.UserId == userId);

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
                await _unitOfWork.Repository<NotificationPreference>().AddAsync(pref);
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
                _unitOfWork.Repository<NotificationPreference>().Update(pref);
            }

            await _unitOfWork.SaveChangesAsync();
            return Result<bool>.Ok(true, "Preferences saved.");
        }
    }
}
