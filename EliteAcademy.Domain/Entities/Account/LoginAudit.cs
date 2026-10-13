namespace EliteAcademy.Domain.Entities.Account
{
    public class LoginAudit
    {
        public Guid Id { get; private set; }
        public string? UserId { get; private set; }
        public DateTime LoginTime { get; private set; }
        public string? IPAddress { get; private set; }
        public string? UserAgent { get; private set; }
        public bool IsSuccessful { get; private set; }
        public string? ErrorMessage { get; private set; }

        private LoginAudit() { } // EF Core

        public static LoginAudit Create(
            string? userId,
            string? ipAddress,
            string? userAgent,
            bool isSuccessful,
            string? errorMessage = null)
        {
            return new LoginAudit
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LoginTime = DateTime.UtcNow,
                IPAddress = ipAddress,
                UserAgent = userAgent,
                IsSuccessful = isSuccessful,
                ErrorMessage = isSuccessful ? null : errorMessage
            };
        }
    }
}
