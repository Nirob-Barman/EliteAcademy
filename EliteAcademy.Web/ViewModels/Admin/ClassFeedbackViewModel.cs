using System.ComponentModel.DataAnnotations;

namespace EliteAcademy.Web.ViewModels.Admin
{
    public class ClassFeedbackViewModel
    {
        public int ClassId { get; set; }

        [Required(ErrorMessage = "Feedback is required when rejecting a class.")]
        [StringLength(500)]
        public string Feedback { get; set; } = string.Empty;
    }
}
