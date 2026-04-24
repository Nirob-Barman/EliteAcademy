namespace EliteAcademy.Domain.Entities
{
    public class AppNotification : BaseEntity
    {
        public string? UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public string? Link { get; set; }
    }
}
