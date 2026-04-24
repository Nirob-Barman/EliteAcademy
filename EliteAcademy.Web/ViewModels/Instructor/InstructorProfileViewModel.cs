using System.ComponentModel.DataAnnotations;

namespace EliteAcademy.Web.ViewModels.Instructor
{
    public class InstructorProfileViewModel
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
