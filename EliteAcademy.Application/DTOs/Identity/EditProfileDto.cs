namespace EliteAcademy.Application.DTOs.Identity
{
    public class EditProfileDto
    {
        public string? FirstName    { get; set; }
        public string? LastName     { get; set; }
        public string? Email        { get; set; }
        public string? PhoneNumber  { get; set; }
        public string? Gender       { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address      { get; set; }
        public string? ImageUrl     { get; set; }
        public string? Role         { get; set; }
    }
}
