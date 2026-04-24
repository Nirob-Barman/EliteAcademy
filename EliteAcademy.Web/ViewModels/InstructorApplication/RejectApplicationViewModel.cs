using System.ComponentModel.DataAnnotations;

namespace EliteAcademy.Web.ViewModels.InstructorApplication
{
    public class RejectApplicationViewModel
    {
        public int ApplicationId { get; set; }

        [Required(ErrorMessage = "A reason is required when rejecting an application.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Reason must be between 10 and 1000 characters.")]
        [Display(Name = "Reason / Feedback")]
        public string AdminNotes { get; set; } = string.Empty;
    }
}
