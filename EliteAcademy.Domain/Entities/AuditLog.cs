namespace EliteAcademy.Domain.Entities
{
    public class AuditLog : BaseEntity
    {
        public string EntityName { get; private set; } = string.Empty;
        public string Action { get; private set; } = string.Empty;
        public string? UserId { get; private set; }
        public string? UserName { get; private set; }
        public string? Details { get; private set; }
        public string? OldValues { get; private set; }
        public string? NewValues { get; private set; }

        private AuditLog() { } // EF Core

        public static AuditLog Create(
            string entityName,
            string action,
            string? userId = null,
            string? userName = null,
            string? details = null,
            string? oldValues = null,
            string? newValues = null)
        {
            if (string.IsNullOrWhiteSpace(entityName))
                throw new ArgumentException("Entity name is required.", nameof(entityName));

            if (string.IsNullOrWhiteSpace(action))
                throw new ArgumentException("Action is required.", nameof(action));

            var auditLog = new AuditLog
            {
                EntityName = entityName.Trim(),
                Action = action.Trim(),
                UserId = userId,
                UserName = userName,
                Details = details,
                OldValues = oldValues,
                NewValues = newValues
            };

            auditLog.CreatedAt = DateTime.UtcNow;

            return auditLog;
        }
    }
}
