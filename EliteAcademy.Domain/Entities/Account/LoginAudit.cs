namespace EliteAcademy.Domain.Entities.Account
{
    public class LoginAudit
    {
        public Guid Id { get; set; }
        public string? UserId { get; set; }
        public DateTime LoginTime { get; set; }
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public bool IsSuccessful { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
