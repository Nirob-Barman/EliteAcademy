namespace EliteAcademy.Application.DTOs.Account
{
    public class LoginHistoryItemDto
    {
        public DateTime LoginTime  { get; set; }
        public string?  IPAddress  { get; set; }
        public string?  UserAgent  { get; set; }
        public bool     IsSuccessful { get; set; }
        public string?  ErrorMessage { get; set; }
    }
}
