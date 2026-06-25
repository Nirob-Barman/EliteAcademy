using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Notification;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IApplicationDbContext _context;
        private readonly IUserContextService _userContextService;

        public NotificationService(
            IApplicationDbContext context,
            IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        public async Task CreateAsync(string userId, string title, string message, string? link = null)
        {
            _context.AppNotifications.Add(new AppNotification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Link = link,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        public async Task<Result<int>> GetUnreadCountAsync()
        {
            var userId = _userContextService.UserId;
            if (userId == null) return Result<int>.Ok(0);

            var count = await _context.AppNotifications.CountAsync(n => n.UserId == userId && !n.IsRead);
            return Result<int>.Ok(count);
        }

        public async Task<Result<List<NotificationDto>>> GetMyAsync()
        {
            var userId = _userContextService.UserId!;
            var items = await _context.AppNotifications.AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    Link = n.Link,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();

            return Result<List<NotificationDto>>.Ok(items);
        }

        public async Task<Result<bool>> MarkAllReadAsync()
        {
            var userId = _userContextService.UserId!;
            var unread = await _context.AppNotifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();

            foreach (var n in unread)
            {
                n.IsRead = true;
                n.UpdatedAt = DateTime.UtcNow;
            }

            if (unread.Any())
                await _context.SaveChangesAsync();

            return Result<bool>.Ok(true);
        }
    }
}
