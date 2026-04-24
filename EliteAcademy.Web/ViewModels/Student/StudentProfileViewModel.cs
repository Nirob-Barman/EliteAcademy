using System.ComponentModel.DataAnnotations;

namespace EliteAcademy.Web.ViewModels.Student
{
    public class StudentProfileViewModel
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public string? Email { get; set; }
        public IFormFile? PhotoFile { get; set; }
        public string? ExistingPhotoUrl { get; set; }
    }
}
