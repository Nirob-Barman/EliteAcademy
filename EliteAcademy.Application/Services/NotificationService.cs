using EliteAcademy.Application.Common.Interfaces;
using EliteAcademy.Application.DTOs.Notification;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities;

namespace EliteAcademy.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IApplicationDbContext _context;
        private readonly IAsyncQueryExecutor _executor;
        private readonly IUserContextService _userContextService;

        public NotificationService(
            IApplicationDbContext context,
            IAsyncQueryExecutor executor,
            IUserContextService userContextService)
        {
            _context            = context;
            _executor           = executor;
            _userContextService = userContextService;
        }

        public async Task CreateAsync(string userId, string title, string message, string? link = null)
        {
            _context.Add(new AppNotification
            {
                UserId    = userId,
                Title     = title,
                Message   = message,
                Link      = link,
                IsRead    = false,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        public async Task<Result<int>> GetUnreadCountAsync()
        {
            var userId = _userContextService.UserId;
            if (userId == null) return Result<int>.Ok(0);

            var count = await _executor.CountAsync(_context.AppNotifications.Where(n => n.UserId == userId && !n.IsRead));
            return Result<int>.Ok(count);
        }

        public async Task<Result<List<NotificationDto>>> GetMyAsync()
        {
            var userId = _userContextService.UserId!;
            var items = await _executor.ToListAsync(_context.AppNotifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .Select(n => new NotificationDto
                {
                    Id        = n.Id,
                    Title     = n.Title,
                    Message   = n.Message,
                    IsRead    = n.IsRead,
                    Link      = n.Link,
                    CreatedAt = n.CreatedAt
                }), noTracking: true);

            return Result<List<NotificationDto>>.Ok(items);
        }

        public async Task<Result<bool>> MarkAllReadAsync()
        {
            var userId = _userContextService.UserId!;
            var unread = await _executor.ToListAsync(_context.AppNotifications.Where(n => n.UserId == userId && !n.IsRead));

            foreach (var n in unread)
            {
                n.IsRead    = true;
                n.UpdatedAt = DateTime.UtcNow;
            }

            if (unread.Any())
                await _context.SaveChangesAsync();

            return Result<bool>.Ok(true);
        }
    }
}
