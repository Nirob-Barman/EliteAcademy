using EliteAcademy.Application.DTOs.Notification;
using EliteAcademy.Application.Wrappers;

namespace EliteAcademy.Application.Interfaces.Services
{
    public interface INotificationPreferenceService
    {
        Task<Result<NotificationPreferenceDto>> GetMyPreferencesAsync();
        Task<Result<bool>> UpdateMyPreferencesAsync(NotificationPreferenceDto dto);
    }
}
