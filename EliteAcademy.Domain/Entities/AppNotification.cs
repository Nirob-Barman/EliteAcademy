namespace EliteAcademy.Domain.Entities
{
    public class AppNotification : BaseEntity
    {
        public string? UserId { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string Message { get; private set; } = string.Empty;
        public bool IsRead { get; private set; }
        public string? Link { get; private set; }

        private AppNotification() { } // EF Core

        public static AppNotification Create(
            string? userId,
            string title,
            string message,
            string? link = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Notification title is required.", nameof(title));

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Notification message is required.", nameof(message));

            var notification = new AppNotification
            {
                UserId = userId,
                Title = title.Trim(),
                Message = message.Trim(),
                Link = link,
                IsRead = false
            };

            notification.CreatedAt = DateTime.UtcNow;

            return notification;
        }

        public void MarkAsRead()
        {
            if (IsRead)
                return;

            IsRead = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsUnread()
        {
            if (!IsRead)
                return;

            IsRead = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
