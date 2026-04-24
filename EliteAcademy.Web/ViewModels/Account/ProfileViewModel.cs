using System.ComponentModel.DataAnnotations;

namespace EliteAcademy.Web.ViewModels.Account
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        public string? Email        { get; set; }
        public string? PhoneNumber  { get; set; }
        public string? Gender       { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public string? Address          { get; set; }
        public string? ExistingPhotoUrl { get; set; }
        public IFormFile? PhotoFile     { get; set; }
        public string? Role             { get; set; }
    }
}
