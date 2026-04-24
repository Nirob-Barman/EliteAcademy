using EliteAcademy.Application.DTOs.Notification;
using EliteAcademy.Application.Wrappers;

namespace EliteAcademy.Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task CreateAsync(string userId, string title, string message, string? link = null);
        Task<Result<int>> GetUnreadCountAsync();
        Task<Result<List<NotificationDto>>> GetMyAsync();
        Task<Result<bool>> MarkAllReadAsync();
    }
}
