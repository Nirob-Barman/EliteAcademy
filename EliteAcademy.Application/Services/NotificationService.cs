using EliteAcademy.Application.DTOs.Notification;
using EliteAcademy.Application.Interfaces;
using EliteAcademy.Application.Interfaces.Persistence;
using EliteAcademy.Application.Interfaces.Services;
using EliteAcademy.Application.Wrappers;
using EliteAcademy.Domain.Entities;

namespace EliteAcademy.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContextService _userContextService;

        public NotificationService(
            IUnitOfWork unitOfWork,
            IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _userContextService = userContextService;
        }

        public async Task CreateAsync(string userId, string title, string message, string? link = null)
        {
            await _unitOfWork.Repository<AppNotification>().AddAsync(new AppNotification
            {
                UserId    = userId,
                Title     = title,
                Message   = message,
                Link      = link,
                IsRead    = false,
                CreatedAt = DateTime.UtcNow
            });
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<Result<int>> GetUnreadCountAsync()
        {
            var userId = _userContextService.UserId;
            if (userId == null) return Result<int>.Ok(0);

            var count = await _unitOfWork.Repository<AppNotification>()
                .CountAsync(n => n.UserId == userId && !n.IsRead);
            return Result<int>.Ok(count);
        }

        public async Task<Result<List<NotificationDto>>> GetMyAsync()
        {
            var userId = _userContextService.UserId!;
            var items = (await _unitOfWork.Repository<AppNotification>()
                .Where(n => n.UserId == userId))
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
                }).ToList();

            return Result<List<NotificationDto>>.Ok(items);
        }

        public async Task<Result<bool>> MarkAllReadAsync()
        {
            var userId = _userContextService.UserId!;
            var unread = (await _unitOfWork.Repository<AppNotification>()
                .Where(n => n.UserId == userId && !n.IsRead)).ToList();

            foreach (var n in unread)
            {
                n.IsRead    = true;
                n.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<AppNotification>().Update(n);
            }

            if (unread.Any())
                await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Ok(true);
        }
    }
}
