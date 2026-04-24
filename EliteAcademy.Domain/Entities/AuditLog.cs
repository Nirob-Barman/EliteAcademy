namespace EliteAcademy.Domain.Entities
{
    public class AuditLog : BaseEntity
    {
        public string EntityName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Details { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
    }
}
